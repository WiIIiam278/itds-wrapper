// Provided freely from https://gist.github.com/neilt6/6d07322070470536ea0ba409c343c2a5

using ITDSWrapper.Audio;
using NAudio.Wave;

namespace ITDSWrapper.Android;

public class AndroidAudioBackend : IAudioBackend
{
    private StreamingWaveProvider? _waveProvider;
    private readonly AndroidAudioPlayer _audioPlayer = new() { DesiredLatency = 30, NumberOfBuffers = 4 };
    private bool _started;

    public void Initialize(double sampleRate)
    {
        _waveProvider = new(new((int)sampleRate, 2));
        _audioPlayer.Init(_waveProvider);
    }
    
    public bool ShouldPlay()
    {
        return !_started;
    }

    public void Play()
    {
        _started = true;
        _audioPlayer.Play();
    }

    public void TogglePause()
    {
        if (_audioPlayer.PlaybackState == PlaybackState.Playing)
        {
            _audioPlayer.Pause();
        }
        else
        {
            _audioPlayer.Play();
        }
    }

    public void AddSamples(byte[] samples)
    {
        _waveProvider?.AddSamples(samples);
    }
}