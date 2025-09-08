using NAudio.Wave;
using PortAudioSharp;

namespace ITDSWrapper.Audio;

public class PortAudioBackend : IAudioBackend
{
    private StreamingWaveProvider _waveProvider;
    private PortAudioWavePlayer _wavePlayer;
    
    public PortAudioBackend(double sampleRate)
    {
        _waveProvider = new(new((int)sampleRate, 2));
        _wavePlayer = new(150);
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