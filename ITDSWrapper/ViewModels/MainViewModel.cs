using System;
using System.IO;
using System.Reflection;
using System.Timers;
using ITDSWrapper.Controls;
using Libretro.NET;
using ReactiveUI.Fody.Helpers;

namespace ITDSWrapper.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RetroWrapper Wrapper { get; }
    [Reactive]
    public EmuImage? CurrentFrame { get; set; }

    private byte[] _frameData = new byte[256 * 384 * 4];
    
    public MainViewModel()
    {
        Wrapper = new();
        Wrapper.LoadCore();
        using Stream ndsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ITDSWrapper.itds.nds")!;
        byte[] data = new byte[ndsStream.Length];
        ndsStream.ReadExactly(data);
        Wrapper.LoadGame(data);
        
        Wrapper.OnFrame = DisplayFrame;
        Wrapper.OnSample = PlaySample;
        Timer timer = new(TimeSpan.FromMilliseconds(1000.0 / 10));
        timer.Elapsed += (_, _) =>
        {
            Wrapper.Run();
        };
        timer.Start();
    }

    private void DisplayFrame(byte[] frame, uint width, uint height)
    {
        Array.Copy(frame, _frameData, frame.Length);
        // if (CurrentFrame is not null)
        // {
        //     CurrentFrame.SetFrame(newFrame);
        //     this.RaisePropertyChanged(nameof(CurrentFrame));
        //     return;
        // }
        CurrentFrame = new(_frameData, width, height);
    }
    
    private void PlaySample(byte[] sample)
    {
        
    }
}