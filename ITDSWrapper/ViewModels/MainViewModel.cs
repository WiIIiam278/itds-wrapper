using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ITDSWrapper.Audio;
using ITDSWrapper.Controls;
using Libretro.NET;
using ReactiveUI.Fody.Helpers;

namespace ITDSWrapper.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RetroWrapper Wrapper { get; }
    [Reactive]
    public EmuImage? CurrentFrame { get; set; }

    private readonly byte[] _frameData = new byte[256 * 384 * 4];
    
    private readonly IAudioBackend _audioBackend;
    
    public MainViewModel()
    {
        Wrapper = new();
        Wrapper.LoadCore();
        using Stream ndsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ITDSWrapper.itds.nds")!;
        byte[] data = new byte[ndsStream.Length];
        ndsStream.ReadExactly(data);
        Wrapper.LoadGame(data);

        if (OperatingSystem.IsWindows())
        {
            _audioBackend = new NAudioWinBackend(Wrapper.SampleRate);
        }
        else
        {
            
        }
        
        Wrapper.OnFrame = DisplayFrame;
        Wrapper.OnSample = PlaySample;
        TimeSpan interval = TimeSpan.FromSeconds(1 / Wrapper.FPS);
        DateTime nextTick = DateTime.Now + interval;
        Task.Run(() =>
        {
            while (true)
            {
                Wrapper.Run();
                while (DateTime.Now < nextTick)
                {
                    Thread.Sleep(nextTick - DateTime.Now);
                }
                nextTick += interval;
            }
        });
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
}