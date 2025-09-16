﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
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
    public EmuImage? CurrentFrame { get; set; }

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

    public bool Closing { get; set; }

    private readonly byte[] _frameData = new byte[256 * 384 * 4];
    
    private readonly PauseDriver _pauseDriver;
    private readonly LogInterpreter? _logInterpreter;
    
    private readonly IAudioBackend _audioBackend;
    private readonly IHapticsBackend? _hapticsBackend;
    
    private readonly IInputDriver _inputDriver;
    private readonly PointerState? _pointerState;

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
        Wrapper = new();
        Wrapper.LoadCore();
        using Stream ndsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ITDSWrapper.itds.nds")!;
        byte[] data = new byte[ndsStream.Length];
        ndsStream.ReadExactly(data);
        Wrapper.LoadGame(data);

        if (((App)Application.Current!).AudioBackend is not null)
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
        
        _logInterpreter = ((App)Application.Current).LogInterpreter ?? new();

        _inputDriver = ((App)Application.Current).InputDriver ?? new DefaultInputDriver(IsMobile);
        _pointerState = new(EmuRenderWidth, EmuRenderHeight);
        if (IsMobile)
        {
            AssignVirtualBindings();
        }
        
        Wrapper.OnFrame = DisplayFrame;
        Wrapper.OnSample = PlaySample;
        Wrapper.OnCheckInput = HandleInput;
        Wrapper.OnRumble = DoRumble;
        Wrapper.OnReceiveLog = HandleLog;
        ThreadPool.QueueUserWorkItem(_ => Run());
    }

    public void HandleKey<T>(T input, bool pressed)
    {
        if (pressed)
        {
            _inputDriver.Push(input);
            
            if (_inputDriver.QueryInput(RetroBindings.RETRO_DEVICE_ID_JOYPAD_R3))
            {
                OpenSettings();
            }
        }
        else
        {
            _inputDriver.Release(input);
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
            updater?.Update();
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
        
        _inputDriver.Shutdown();
        Wrapper.Dispose();
    }

    private void DisplayFrame(byte[] frame, uint width, uint height)
    {
        Array.Copy(frame, _frameData, frame.Length);
        CurrentFrame = new(_frameData, width, height);
    }
    
    private void PlaySample(byte[] sample)
    {
        _audioBackend.PlaySamples(sample);
    }

    private short HandleInput(uint port, uint device, uint index, uint id)
    {
        if (device == RetroBindings.RETRO_DEVICE_JOYPAD)
        {
            return _inputDriver.QueryInput(id) ? (short)1 : (short)0;
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

    private bool DoRumble(uint port, uint type, ushort strength)
    {
        _inputDriver.DoRumble(strength);
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

    private void AssignVirtualBindings()
    {
        foreach (uint inputKey in _inputDriver.GetInputKeys())
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
                case RetroBindings.RETRO_DEVICE_ID_JOYPAD_R3:
                    SettingsButton = new("*", button, 25, 25, _hapticsBackend);
                    break;
                default:
                    button = null;
                    break;
            }
            _inputDriver.SetBinding(inputKey, button);
        }
    }
}