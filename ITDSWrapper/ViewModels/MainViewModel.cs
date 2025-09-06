using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using ITDSWrapper.Controls;
using Libretro.NET;
using NAudio.Sdl2;
using NAudio.Wave;
using ReactiveUI.Fody.Helpers;

namespace ITDSWrapper.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RetroWrapper Wrapper { get; }
    [Reactive]
    public EmuImage? CurrentFrame { get; set; }

    private byte[] _frameData = new byte[256 * 384 * 4];

    private BufferedWaveProvider _waveProvider;
    private WaveOutSdl _waveOut;
    
    public MainViewModel()
    {
        Wrapper = new();
        Wrapper.LoadCore();
        using Stream ndsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ITDSWrapper.itds.nds")!;
        byte[] data = new byte[ndsStream.Length];
        ndsStream.ReadExactly(data);
        Wrapper.LoadGame(data);

        _waveProvider = new(new((int)Wrapper.SampleRate, 2))
        {
            BufferLength = 65536,
        };
        _waveOut = new()
        {
            DesiredLatency = 100,
        };
        _waveOut.Init(_waveProvider);
        
        Wrapper.OnFrame = DisplayFrame;
        Wrapper.OnSample = PlaySample;
        Task.Run((async Task () =>
        {
            Stopwatch stopwatch = new();
            _waveOut.Play();
            while (true)
            {
                stopwatch.Start();
                Wrapper.Run();
                stopwatch.Stop();
                if (1000 / Wrapper.FPS > stopwatch.ElapsedMilliseconds)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000 / Wrapper.FPS - stopwatch.ElapsedMilliseconds));
                }
            }
        })!);
    }

    private void DisplayFrame(byte[] frame, uint width, uint height)
    {
        Array.Copy(frame, _frameData, frame.Length);
        CurrentFrame = new(_frameData, width, height);
    }
    
    private void PlaySample(byte[] sample)
    {
        _waveProvider.AddSamples(sample, 0, sample.Length);
        // if (_waveProvider.BufferedBytes > 10000 && _waveOut.PlaybackState != PlaybackState.Playing)
        // {
        // }
    }
}