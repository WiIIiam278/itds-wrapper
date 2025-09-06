using NAudio.Sdl2;
using NAudio.Wave;

namespace ITDSWrapper.Audio;

public class NAudioSdlBackend : IAudioBackend
{
    private readonly StreamingWaveProvider _waveProvider;
    private readonly WaveOutSdl _waveOut;

    public NAudioSdlBackend(double sampleRate)
    {
        _waveProvider = new(new((int)sampleRate, 2));
        _waveOut = new();
        _waveOut.Init(_waveProvider);
    }

    public bool ShouldPlay()
    {
        return _waveOut.PlaybackState != PlaybackState.Playing;
    }

    public void Play()
    {
        _waveOut.Play();
    }

    public void AddSamples(byte[] samples)
    {
        _waveProvider.AddSamples(samples);
    }
}