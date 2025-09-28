using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
    
    public RetroWrapper Wrapper { get; }
    [Reactive]
    public EmuImageSource? CurrentFrame { get; set; }

    private readonly Settings _settings;
    private int _currentLangBit;
    private int _langInputsSent;

    private double _emuRenderWidth = 256;
    private double _emuRenderHeight = 384;

    public double EmuRenderWidth
    {
        get => _emuRenderWidth;
        set
        {
            this.RaiseAndSetIfChanged(ref _emuRenderWidth, value);
            if (_pointerState is not null)
            {
                _pointerState.Width = value;
            }
        }
    }

    public double EmuRenderHeight
    {
        get => _emuRenderHeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _emuRenderHeight, value);
            if (_pointerState is not null)
            {
                _pointerState.Height = value;
            }
        }
    }

    public int TopPadding => IsMobile ? 10 : 0;
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
    private string _currentBorder = "TITLE_BG";
    private int _currentBorderFrame;
    private string _nextBorder = string.Empty;
    private int _nextBorderFrame;
    
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
    
    private readonly IBatteryMonitor? _batteryMonitor;
    private System.Timers.Timer _batteryTimer;

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
    public VirtualButtonViewModel? StartButton { get; set; }
    public VirtualButtonViewModel? SelectButton { get; set; }
    public VirtualButtonViewModel? SettingsButton { get; set; }

    public bool DisplaySettingsOverlay { get; set; }
    [Reactive]
    public IEffect? ScreenEffect { get; set; }
    
    public MainViewModel()
    {
        _settings = Settings.Load(IsMobile
            ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
            : OperatingSystem.IsMacOS()
                ? Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.FullName)!.FullName)!.FullName)!.FullName
                : AppDomain.CurrentDomain.BaseDirectory);
        
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
        _inputDrivers.Insert(0, new DefaultInputDriver(IsMobile, OpenSettings));
        _pointerState = new(EmuRenderWidth, EmuRenderHeight);
        if (IsMobile)
        {
            AssignVirtualBindings();
        }
        
        _batteryMonitor = ((App)Application.Current).BatteryMonitor;
        Wrapper.BatteryLevel = _batteryMonitor?.GetBatteryLevel() ?? 100;
        _batteryTimer = new(TimeSpan.FromMinutes(1)) { AutoReset = true };
        _batteryTimer.Elapsed += (_, _) =>
        {
            Wrapper.BatteryLevel = _batteryMonitor?.GetBatteryLevel() ?? 100;
        };
        _batteryTimer.Start();
        
        Wrapper.OnFrame = DisplayFrame;
        Wrapper.OnSample = PlaySample;
        Wrapper.OnCheckInput = HandleStartupInput;
        Wrapper.OnRumble = DoRumble;
        ThreadPool.QueueUserWorkItem(_ => Run());
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
        TimeSpan interval = TimeSpan.FromSeconds(1 / Wrapper.FPS);
        DateTime nextTick = DateTime.Now + interval;
        IUpdater? updater = ((App)Application.Current!).Updater;
        
        while (!Closing)
        {
            int nextInputDriver = updater?.Update() ?? -1;
            if (nextInputDriver >= 0)
            {
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

    private void OpenSettings()
    {
        DisplaySettingsOverlay = !DisplaySettingsOverlay;
        _pauseDriver.PushPauseState(DisplaySettingsOverlay);
        ScreenEffect = ScreenEffect is null ? new BlurEffect { Radius = 30 } : null;
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
        if (CurrentBorderOpacity < 0.01)
        {
            CurrentBorder = NextBorder;
            NextBorder = null;
            CurrentBorderOpacity = 1.0;
            NextBorderOpacity = 0.0;
            _nextBorder = string.Empty;
            _nextBorderFrame = 0;
            return;
        }

        CurrentBorderOpacity -= 0.01;
        NextBorderOpacity += 0.01;
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
                    LButton = new("L", button, 75, 40, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_R:
                    RButton = new("R", button, 75, 40, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP:
                    UpButton = new("・", button, 25, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT:
                    RightButton = new("・", button, 50, 25, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN:
                    DownButton = new("・", button, 25, 50, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT:
                    LeftButton = new("・", button, 50, 25, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_START:
                    StartButton = new("START", button, 50, 25, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT:
                    SelectButton = new("SELECT", button, 50, 25, _hapticsBackend);
                    break;
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_R2:
                    button.SpecialAction = OpenSettings;
                    SettingsButton = new("*", button, 25, 25, _hapticsBackend);
                    break;
                default:
                    button = null;
                    break;
            }
            _inputDrivers[defaultInputDriverIndex].SetBinding(inputKey, button);
        }
    }
}