using NAudio.Wave;

namespace ITDSWrapper.Audio;

public class NAudioSilkNetOpenALBackend : IAudioBackend
{
    private StreamingWaveProvider _waveProvider;
    private SilkNetOpenALWavePlayer _wavePlayer;
    
    public NAudioSilkNetOpenALBackend(double sampleRate)
    {
        _waveProvider = new(new((int)sampleRate, 2));
        _wavePlayer = new() { DesiredLatency = 30, NumberOfBuffers = 4};
        _wavePlayer.Init(_waveProvider);
    }

    public void Initialize(double sampleRate)
    {
    }
    
    public bool ShouldPlay()
    {
        return _wavePlayer.PlaybackState != PlaybackState.Playing;
    }

    public void Play()
    {
        _wavePlayer.Play();
    }

    public void TogglePause()
    {
        _wavePlayer.Pause();
    }

    public void AddSamples(byte[] samples)
    {
        _waveProvider.AddSamples(samples);
    }
}