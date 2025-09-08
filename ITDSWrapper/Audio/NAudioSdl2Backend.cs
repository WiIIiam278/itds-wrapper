using NAudio.Sdl2;
using NAudio.Wave;

namespace ITDSWrapper.Audio;

public class NAudioSdl2Backend : IAudioBackend
{
    private readonly StreamingWaveProvider _waveProvider;
    private readonly WaveOutSdl _waveOut;

    public NAudioSdl2Backend(double sampleRate)
    {
        _waveProvider = new(new((int)sampleRate, 2));
        _waveOut = new();
        _waveOut.Init(_waveProvider);
    }
    
    public void Initialize(double sampleRate)
    {
    }

    public bool ShouldPlay()
    {
        return _waveOut.PlaybackState != PlaybackState.Playing;
    }

    public void Play()
    {
        _waveOut.Play();
    }

    public void TogglePause()
    {
        _waveOut.Pause();
    }

    public void AddSamples(byte[] samples)
    {
        _waveProvider.AddSamples(samples);
    }
}