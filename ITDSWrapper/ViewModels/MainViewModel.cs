using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ITDSWrapper.Controls;
using ITDSWrapper.Models;
using Libretro.NET;
using NAudio.Sdl2;
using NAudio.Sdl2.Structures;
using NAudio.Wave;
using ReactiveUI.Fody.Helpers;

namespace ITDSWrapper.ViewModels;

public class MainViewModel : ViewModelBase
{
    public RetroWrapper Wrapper { get; }
    [Reactive]
    public EmuImage? CurrentFrame { get; set; }

    private readonly byte[] _frameData = new byte[256 * 384 * 4];

    private readonly StreamingWaveProvider _waveProvider;
    private readonly WaveOutSdl _waveOut;
    
    public MainViewModel()
    {
        Wrapper = new();
        Wrapper.LoadCore();
        using Stream ndsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ITDSWrapper.itds.nds")!;
        byte[] data = new byte[ndsStream.Length];
        ndsStream.ReadExactly(data);
        Wrapper.LoadGame(data);

        _waveProvider = new(new((int)Wrapper.SampleRate, 2));
        _waveOut = new();
        _waveOut.Init(_waveProvider);
        
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
        _waveProvider.AddSamples(sample);
        if (_waveOut.PlaybackState != PlaybackState.Playing)
        {
            _waveOut.Play();
        }
    }
}