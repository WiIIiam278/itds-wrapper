using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using ITDSWrapper.Audio;
using ITDSWrapper.Controls;
using ITDSWrapper.Input;
using Libretro.NET;
using Libretro.NET.Bindings;
using ReactiveUI.Fody.Helpers;

namespace ITDSWrapper.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RetroWrapper Wrapper { get; }
    [Reactive]
    public EmuImage? CurrentFrame { get; set; }

    public bool Closing { get; set; } = false;

    private readonly byte[] _frameData = new byte[256 * 384 * 4];
    
    private readonly IAudioBackend _audioBackend;
    
    private readonly InputBindings _inputBindings;
    private readonly PointerState _pointerState;
    
    public bool DisplaySettingsOverlay { get; set; }
    [Reactive]
    public IEffect? ScreenEffect { get; set; }

    public bool Paused => DisplaySettingsOverlay;

    public bool DisplayInputOverlay { get; set; }
    
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
        else if (OperatingSystem.IsWindows())
        {
            _audioBackend = new NAudioWinBackend(Wrapper.SampleRate);
        }
        else
        {
            _audioBackend = new NAudioSilkNetOpenALBackend(Wrapper.SampleRate);
        }

        _inputBindings = new();
        _pointerState = new();
        
        Wrapper.OnFrame = DisplayFrame;
        Wrapper.OnSample = PlaySample;
        Wrapper.OnCheckInput = HandleInput;
        ThreadPool.QueueUserWorkItem(_ => Run());
    }

    public void HandleKey(PhysicalKey key, bool pressed)
    {
        if (pressed)
        {
            _inputBindings.PushKey(key);
            
            if (_inputBindings.QueryInput(RetroBindings.RETRO_DEVICE_ID_JOYPAD_R3))
            {
                DisplaySettingsOverlay = !DisplaySettingsOverlay;
                ScreenEffect = ScreenEffect is null ? new BlurEffect { Radius = 30 } : null;
                _audioBackend.TogglePause();
            }
        }
        else
        {
            _inputBindings.ReleaseKey(key);
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
            _pointerState.Press(pos.X, pos.Y);
        }
        else if (releasedArgs is not null)
        {
            _pointerState.Release();
        }
        else if (movedArgs is not null && _pointerState.Pressed)
        {
            Point pos = movedArgs.GetCurrentPoint(relativeTo).Position;
            _pointerState.Press(pos.X, pos.Y);
        }
    }

    private void Run()
    {
        TimeSpan interval = TimeSpan.FromSeconds(1 / Wrapper.FPS);
        DateTime nextTick = DateTime.Now + interval;
        
        while (!Closing)
        {
            if (!Paused)
            {
                Wrapper.Run();
                while (DateTime.Now < nextTick)
                {
                    Thread.Sleep(nextTick - DateTime.Now);
                }
                nextTick += interval;
            }
        }
    }

    private void DisplayFrame(byte[] frame, uint width, uint height)
    {
        Array.Copy(frame, _frameData, frame.Length);
        CurrentFrame = new(_frameData, width, height);
    }
    
    private void PlaySample(byte[] sample)
    {
        _audioBackend.AddSamples(sample);
        if (_audioBackend.ShouldPlay())
        {
            _audioBackend.Play();
        }
    }

    private short HandleInput(uint port, uint device, uint index, uint id)
    {
        if (device == RetroBindings.RETRO_DEVICE_JOYPAD)
        {
            return _inputBindings.QueryInput(id) ? (short)1 : (short)0;
        }

        if (device == RetroBindings.RETRO_DEVICE_POINTER)
        {
            switch (id)
            {
                case RetroBindings.RETRO_DEVICE_ID_POINTER_X:
                    return _pointerState.RetroX;
                
                case RetroBindings.RETRO_DEVICE_ID_POINTER_Y:
                    return _pointerState.RetroY;
                
                case RetroBindings.RETRO_DEVICE_ID_POINTER_PRESSED:
                    return _pointerState.Pressed ? (short)1 : (short)0;
            }
        }

        return 0;
    }
}