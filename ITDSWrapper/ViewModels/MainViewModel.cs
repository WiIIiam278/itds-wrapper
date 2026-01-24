using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DiscUtils.Streams;
using ITDSWrapper.Assets;
using ITDSWrapper.Audio;
using ITDSWrapper.Core;
using ITDSWrapper.Graphics;
using ITDSWrapper.Haptics;
using ITDSWrapper.Input;
using ITDSWrapper.ViewModels.Controls;
using Libretro.NET;
using Libretro.NET.Bindings;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace ITDSWrapper.ViewModels;

public class MainViewModel : ViewModelBase
{
    public static bool IsMobile => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
    
    public Control? Top { get; set; }
    
    [Reactive]
    public WindowState WindowState { get; set; } = WindowState.FullScreen;
    [Reactive]
    public SystemDecorations Decorations { get; set; } = SystemDecorations.Full;
    [Reactive]
    public ExtendClientAreaChromeHints ChromeHints { get; set; } = ExtendClientAreaChromeHints.Default;
    [Reactive] 
    public bool ExtendClientArea { get; set; } = false;
    [Reactive]
    public string WindowingModeDesc { get; set; } = Strings.WindowStateFullScreen;

    public enum WindowingMode
    {
        FULL_SCREEN,
        BORDERLESS,
        WINDOWED,
    }
    public int WindowingModeIdx
    {
        get => field;
        set
        {
            field = value;
            if (value is >= 0 and < 3)
            {
                WindowingModeDesc = new[] { Strings.WindowStateFullScreen, Strings.WindowStateBorderless, Strings.WindowStateWindowed }[value];
            }

            switch ((WindowingMode)WindowingModeIdx)
            {
                default:
                case WindowingMode.FULL_SCREEN:
                    WindowState = WindowState.FullScreen;
                    break;
                
                case WindowingMode.BORDERLESS:
                    WindowState = WindowState.Maximized;
                    Decorations = SystemDecorations.None;
                    ChromeHints = ExtendClientAreaChromeHints.NoChrome;
                    ExtendClientArea = true;
                    break;
                
                case WindowingMode.WINDOWED:
                    WindowState = WindowState.Maximized;
                    Decorations = SystemDecorations.Full;
                    ChromeHints = ExtendClientAreaChromeHints.Default;
                    ExtendClientArea = false;
                    break;
            }
        }
    }
    
    public RetroWrapper Wrapper { get; }
    [Reactive]
    public EmuImageSource? CurrentFrame { get; set; }
    
    [Reactive] 
    public bool DisplaySettingsMenuOpen { get; set; }
    [Reactive] 
    public bool ControllerSettingsMenuOpen { get; set; }
    [Reactive]
    public bool LegalMenuOpen { get; set; }
    public bool ShowSidebar => !(IsMobile && (DisplaySettingsMenuOpen || ControllerSettingsMenuOpen || LegalMenuOpen));
    
    private readonly Settings _settings;
    private int _currentLangBit;
    private int _langInputsSent;

    public double EmuRenderWidth
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _pointerState?.Width = value;
        }
    } = 256;

    public double EmuRenderHeight
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _pointerState?.Height = value;
        }
    } = 384;

    public int TopPadding
    {
        get => IsMobile ? field : 0;
        set;
    } = 45;

    public int BottomPadding
    {
        get => IsMobile ? field : 0;
        set;
    } = 10;
    
    public int MenuMargins => IsMobile ? 30 : 50;

    public bool DisplayVirtualControls => IsMobile && CurrentInputDriver == 0;

    [Reactive]
    public Bitmap? CurrentBorder { get; set; }
    [Reactive]
    public Bitmap? NextBorder { get; set; }
    [Reactive]
    public double CurrentBorderOpacity { get; set; } = 1.0;
    [Reactive]
    public double NextBorderOpacity { get; set; }
    private System.Timers.Timer? _borderTimer;
    private string _currentBorder = "BLACK_BG";
    private int _currentBorderFrame;
    private string _nextBorder = string.Empty;
    private int _nextBorderFrame = 0;
    
    public bool Closing { get; set; }
    
    private readonly PauseDriver _pauseDriver;
    private readonly LogInterpreter? _logInterpreter;
    
    private readonly IAudioBackend _audioBackend;
    private readonly IHapticsBackend? _hapticsBackend;
    
    private readonly List<IInputDriver> _inputDrivers;
    public int NumInputDrivers => _inputDrivers.Count;
    private int _currentInputDriver;

    public int CurrentInputDriver
    {
        get => _currentInputDriver;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentInputDriver, value);
            this.RaisePropertyChanged(nameof(DisplayVirtualControls));
        }
    }
    private readonly PointerState? _pointerState;

    private readonly System.Timers.Timer _batteryTimer;

    public ICommand CloseMenuOverlayCommand { get; }
    public ICommand OpenDisplaySettingsMenuCommand { get; }
    public ICommand OpenControllerSettingsMenuCommand { get; }
    public ICommand OpenSurveyUrlCommand { get; }
    public ICommand OpenLegalMenuCommand { get; }
    public ICommand OpenDiscordUrlCommand { get; }
    public ICommand QuitToDesktopCommand { get; }
    
    public ICommand ChangeWindowingSettingsCommand { get; }
    
    public VirtualButtonViewModel? AButton { get; set; }
    public VirtualButtonViewModel? BButton { get; set; }
    public VirtualButtonViewModel? XButton { get; set; }
    public VirtualButtonViewModel? YButton { get; set; }
    public VirtualButtonViewModel? LButton { get; set; }
    public VirtualButtonViewModel? RButton { get; set; }
    public VirtualButtonViewModel? UpButton { get; set; }
    public VirtualButtonViewModel? RightButton { get; set; }
    public VirtualButtonViewModel? DownButton { get; set; }
    public VirtualButtonViewModel? LeftButton { get; set; }
    public VirtualMultiButtonViewModel? UpLeftButton { get; set; }
    public VirtualMultiButtonViewModel? UpRightButton { get; set; }
    public VirtualMultiButtonViewModel? DownLeftButton { get; set; }
    public VirtualMultiButtonViewModel? DownRightButton { get; set; }
    
    public VirtualButtonViewModel? StartButton { get; set; }
    public VirtualButtonViewModel? SelectButton { get; set; }
    public VirtualButtonViewModel? SettingsButton { get; set; }
    
    [Reactive]
    public bool DisplaySettingsOverlay { get; set; }
    [Reactive]
    public IEffect? ScreenEffect { get; set; }
    
    public MainViewModel()
    {
        _settings = Settings.Load(RetroWrapper.GetDirectoryForPlatform("settings"));
        
        Wrapper = new();
        _logInterpreter = ((App)Application.Current!).LogInterpreter ?? new();
        _logInterpreter.SetNextBorder = border =>
        {
            _nextBorder = border;
            SetNextBorder();
        };
        if (_settings.ScreenReaderEnabled)
        {
            StartScreenReader();
        }
        Wrapper.OnReceiveLog = HandleLog;
        Wrapper.LoadCore();
        using Stream ndsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ITDSWrapper.itds.nds")!;
        byte[] data = new byte[ndsStream.Length];
        ndsStream.ReadExactly(data);
        Wrapper.LoadGame(data);

        if (_settings.BordersEnabled)
        {
            StartBorder();
        }

        if (((App)Application.Current).AudioBackend is not null)
        {
            _audioBackend = ((App)Application.Current).AudioBackend!;
            _audioBackend.Initialize(Wrapper.SampleRate);
        }
        else
        {
            _audioBackend = new SilkNetOpenALBackend(Wrapper.SampleRate, 32);
        }
        
        if (((App)Application.Current).HapticsBackend is not null)
        {
            _hapticsBackend = ((App)Application.Current).HapticsBackend!;
            _hapticsBackend.Initialize();
        }
        
        _pauseDriver = ((App)Application.Current).PauseDriver ?? new();
        _pauseDriver.AudioBackend = _audioBackend;

        _inputDrivers = ((App)Application.Current).InputDrivers ?? [];
        _inputDrivers.Insert(0, new DefaultInputDriver(IsMobile, ToggleMenuOverlay));
        _pointerState = new(EmuRenderWidth, EmuRenderHeight);
        if (IsMobile)
        {
            AssignVirtualBindings();
        }
        
        InputSwitcher? inputSwitcher = ((App)Application.Current).InputSwitcher;
        inputSwitcher?.Wrapper = Wrapper;
        
        IBatteryMonitor? batteryMonitor = ((App)Application.Current).BatteryMonitor;
        Wrapper.BatteryLevel = batteryMonitor?.GetBatteryLevel() ?? 100;
        _batteryTimer = new(TimeSpan.FromMinutes(1)) { AutoReset = true };
        _batteryTimer.Elapsed += (_, _) =>
        {
            Wrapper.BatteryLevel = batteryMonitor?.GetBatteryLevel() ?? 100;
        };
        _batteryTimer.Start();

        CloseMenuOverlayCommand = ReactiveCommand.Create(ToggleMenuOverlay);
        OpenDisplaySettingsMenuCommand = ReactiveCommand.Create(OpenDisplaySettings);
        OpenControllerSettingsMenuCommand = ReactiveCommand.Create(OpenControllerSettings);
        OpenSurveyUrlCommand = ReactiveCommand.Create(() => OpenUrl("https://google.com"));
        OpenLegalMenuCommand = ReactiveCommand.Create(OpenLegal);
        OpenDiscordUrlCommand = ReactiveCommand.Create(() => OpenUrl("https://discord.com"));
        QuitToDesktopCommand = ReactiveCommand.Create(CloseApplication);
        
        ChangeWindowingSettingsCommand = ReactiveCommand.Create<bool>(ChangeWindowingSettings);
        
        Wrapper.OnFrame = DisplayFrame;
        Wrapper.OnSample = PlaySample;
        inputSwitcher?.SetDefaultInputDelegate(HandleInput);
        Wrapper.OnCheckInput = HandleStartupInput;
        Wrapper.OnRumble = DoRumble;
        ThreadPool.QueueUserWorkItem(_ => Run());
    }

    private void CloseApplication()
    {
        // todo a modal warning?
        Environment.Exit(0);
    }

    private void OpenDisplaySettings()
    {
        DisplaySettingsMenuOpen = true;
        LegalMenuOpen = ControllerSettingsMenuOpen = false;
    }
    private void OpenControllerSettings()
    {
        ControllerSettingsMenuOpen = true;
        LegalMenuOpen = DisplaySettingsMenuOpen = false;
    }
    private void OpenLegal()
    {
        LegalMenuOpen = true;
        DisplaySettingsMenuOpen = ControllerSettingsMenuOpen = false;
    }

    private void ToggleMenuOverlay()
    {
        DisplaySettingsOverlay = !DisplaySettingsOverlay;
        _pauseDriver.PushPauseState(DisplaySettingsOverlay);
        DisplaySettingsMenuOpen = ControllerSettingsMenuOpen = LegalMenuOpen = false;
        ScreenEffect = ScreenEffect is null ? new BlurEffect { Radius = 50 } : null;
    }

    private void OpenUrl(string url)
    {
        TopLevel.GetTopLevel(Top)?.Launcher.LaunchUriAsync(new(url));
    }

    private void ChangeWindowingSettings(bool forward)
    {
        if (!forward && WindowingModeIdx == 0)
        {
            WindowingModeIdx = 2;
        }
        else
        {
            WindowingModeIdx = (WindowingModeIdx + (forward ? 1 : -1)) % 3;
        }
    }

    public void HandleKey<T>(T input, bool pressed)
    {
        for (int i = 0; i < _inputDrivers.Count; i++)
        {
            if (_inputDrivers[i] is DefaultInputDriver)
            {
                CurrentInputDriver = i;
                break;
            }
        }
        if (_inputDrivers[CurrentInputDriver] is not DefaultInputDriver)
        {
            return;
        }
        
        if (pressed)
        {
            _inputDrivers[CurrentInputDriver].Push(input);
        }
        else
        {
            _inputDrivers[CurrentInputDriver].Release(input);
        }
    }

    public void HandlePointer(Visual relativeTo, 
        PointerPressedEventArgs? pressedArgs = null,
        PointerReleasedEventArgs? releasedArgs = null,
        PointerEventArgs? movedArgs = null)
    {
        if (pressedArgs is not null)
        {
            Point pos = pressedArgs.GetCurrentPoint(relativeTo).Position;
            _pointerState?.Press(pos.X, pos.Y);
        }
        else if (releasedArgs is not null)
        {
            _pointerState?.Release();
        }
        else if (movedArgs is not null && (_pointerState?.Pressed ?? false))
        {
            Point pos = movedArgs.GetCurrentPoint(relativeTo).Position;
            _pointerState.Press(pos.X, pos.Y);
        }
    }

    private void Run()
    {
        TimeSpan interval = TimeSpan.FromSeconds(1 / Wrapper.Fps);
        DateTime nextTick = DateTime.Now + interval;
        IUpdater? updater = ((App)Application.Current!).Updater;
        
        while (!Closing)
        {
            int nextInputDriver = updater?.Update() ?? -1;
            if (nextInputDriver >= 0)
            {
                if (nextInputDriver != _currentInputDriver)
                {
                    _inputDrivers[nextInputDriver].RequestInputUpdate = true;
                }
                CurrentInputDriver = nextInputDriver;
            }
            
            if (!_pauseDriver.IsPaused())
            {
                Wrapper.Run();
                
                while (DateTime.Now < nextTick)
                {
                    TimeSpan sleep = nextTick - DateTime.Now;
                    Thread.Sleep(sleep > TimeSpan.Zero ? sleep : TimeSpan.Zero);
                }
                nextTick += interval;
            }
            else
            {
                nextTick = DateTime.Now + interval;
            }
        }

        foreach (IInputDriver inputDriver in _inputDrivers)
        {
            inputDriver.Shutdown();
        }
        Wrapper.Dispose();
        _logInterpreter?.Dispose();
    }

    private void DisplayFrame(byte[] frame, uint width, uint height)
    {
        CurrentFrame ??= new(frame, width, height);
        CurrentFrame.SetFrame(frame);
    }
    
    private void PlaySample(byte[] sample)
    {
        _audioBackend.PlaySamples(sample);
    }

    private short HandleInput(uint port, uint device, uint index, uint id)
    {
        if (device == RetroBindings.RETRO_DEVICE_JOYPAD)
        {
            // Press the debug button when we've changed controller bindings
            if (id == RetroBindings.RETRO_DEVICE_ID_JOYPAD_L3 && _inputDrivers[CurrentInputDriver].RequestInputUpdate)
            {
                _inputDrivers[CurrentInputDriver].RequestInputUpdate = false;
                return 1;
            }
            return _inputDrivers[CurrentInputDriver].QueryInput(id) ? (short)1 : (short)0;
        }

        if (device == RetroBindings.RETRO_DEVICE_POINTER)
        {
            switch (id)
            {
                case RetroBindings.RETRO_DEVICE_ID_POINTER_X:
                    return _pointerState?.RetroX ?? 0;
                
                case RetroBindings.RETRO_DEVICE_ID_POINTER_Y:
                    return _pointerState?.RetroY ?? 0;
                
                case RetroBindings.RETRO_DEVICE_ID_POINTER_PRESSED:
                    return (_pointerState?.Pressed ?? false) ? (short)1 : (short)0;
            }
        }

        return 0;
    }

    private short HandleStartupInput(uint port, uint device, uint index, uint id)
    {
        if ((_logInterpreter?.StartupReceived ?? false) && device == RetroBindings.RETRO_DEVICE_JOYPAD)
        {
            switch (id)
            {
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP:
                    if (_langInputsSent < 4)
                    {
                        _langInputsSent++;
                    }
                    else
                    {
                        return 0;
                    }
                    return (_settings.LanguageIndex & (0x1 << (6 - _currentLangBit))) != 0 ? (short)1 : (short)0;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN:
                    if (_langInputsSent < 4)
                    {
                        _langInputsSent++;
                    }
                    else
                    {
                        return 0;
                    }
                    return (_settings.LanguageIndex & (0x1 << (6 - _currentLangBit))) == 0 ? (short)1 : (short)0;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT:
                    if (_langInputsSent is >= 4 and < 6)
                    {
                        _langInputsSent++;
                        return 1;
                    }
                    if (_langInputsSent >= 6)
                    {
                        _langInputsSent = 0;
                        _currentLangBit++;
                    }
                    return 0;
            }
        }
        
        if (_logInterpreter?.LangReceived ?? false)
        {
            Wrapper.OnCheckInput = HandleInput;
            return 0;
        }

        if (device == RetroBindings.RETRO_DEVICE_JOYPAD)
        {
            if (id is RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP or RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN)
            {
                return 1;
            }
        }
        
        return 0;
    }

    private bool DoRumble(uint port, uint type, ushort strength)
    {
        _inputDrivers[CurrentInputDriver].DoRumble(strength);
        return true;
    }

    private void HandleLog(string line)
    {
        _logInterpreter?.InterpretLog(line);
    }
    
    private void StartScreenReader()
    {
        _logInterpreter!.ScreenReader = ((App)Application.Current!).LogInterpreter?.ScreenReader;
    }

    private void StopScreenReader()
    {
        _logInterpreter!.ScreenReader = null;
    }
    
    private void StartBorder()
    {
        SetBorder();
        _borderTimer = new(TimeSpan.FromMilliseconds(100)) { AutoReset = true };
        _borderTimer.Elapsed += (_, _) =>
        {
            _currentBorderFrame = (_currentBorderFrame + 1) % 600;
            SetBorder();
        };
        _borderTimer.Start();
    }

    private void StopBorder()
    {
        CurrentBorder = null;
        _borderTimer?.Stop();
    }

    private void SetBorder()
    {
        using Stream borderStream = AssetLoader.Open(new($"avares://ITDSWrapper/Assets/Borders/{_currentBorder}/{_currentBorderFrame + 1:0000}.jpg"));
        CurrentBorder = new(borderStream);

        if (NextBorder is not null)
        {
            SetNextBorder();
            FadeBorder();
        }
    }

    private void SetNextBorder()
    {
        using Stream borderStream = AssetLoader.Open(new($"avares://ITDSWrapper/Assets/Borders/{_nextBorder}/{_nextBorderFrame + 1:0000}.jpg"));
        NextBorder = new(borderStream);
    }

    private void FadeBorder()
    {
        if (CurrentBorderOpacity < 0.02)
        {
            CurrentBorder = NextBorder;
            NextBorder = null;
            CurrentBorderOpacity = 1.0;
            NextBorderOpacity = 0.0;
            _currentBorder = _nextBorder;
            _currentBorderFrame = _nextBorderFrame;
            _nextBorder = string.Empty;
            _nextBorderFrame = 0;
            return;
        }

        CurrentBorderOpacity -= 0.02;
        NextBorderOpacity += 0.02;
    }

    private void AssignVirtualBindings()
    {
        int defaultInputDriverIndex = 0;
        foreach (uint inputKey in _inputDrivers[defaultInputDriverIndex].GetInputKeys())
        {
            VirtualButtonInput? button = new();
            switch (inputKey)
            {
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_A:
                    AButton = new("A", button, 50, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_B:
                    BButton = new("B", button, 50, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_X:
                    XButton = new("X", button, 50, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y:
                    YButton = new("Y", button, 50, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_L:
                    LButton = new("L", button, 67, 40, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_R:
                    RButton = new("R", button, 67, 40, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP:
                    UpButton = new("・", button, 50, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT:
                    RightButton = new("・", button, 50, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN:
                    DownButton = new("・", button, 50, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT:
                    LeftButton = new("・", button, 50, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_START:
                    StartButton = new("START", button, 65, 40, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT:
                    SelectButton = new("SELECT", button, 65, 40, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_R2:
                    button.SpecialAction = ToggleMenuOverlay;
                    SettingsButton = new("MENU", button, 65, 40, _hapticsBackend);
                    break;
                default:
                    button = null;
                    break;
            }

            // Multi buttons
            if (UpButton is not null && LeftButton is not null)
            {
                UpLeftButton = new VirtualMultiButtonViewModel("・", [UpButton, LeftButton], 50, 50, _hapticsBackend);
            }
            if (UpButton is not null && RightButton is not null)
            {
                UpRightButton = new VirtualMultiButtonViewModel("・", [UpButton, RightButton], 50, 50, _hapticsBackend);
            }
            if (DownButton is not null && LeftButton is not null)
            {
                DownLeftButton = new VirtualMultiButtonViewModel("・", [DownButton, LeftButton], 50, 50, _hapticsBackend);
            }
            if (DownButton is not null && RightButton is not null)
            {
                DownRightButton = new VirtualMultiButtonViewModel("・", [DownButton, RightButton], 50, 50, _hapticsBackend);
            }
            
            _inputDrivers[defaultInputDriverIndex].SetBinding(inputKey, button);
        }
    }
}