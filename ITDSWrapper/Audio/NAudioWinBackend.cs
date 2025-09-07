namespace ITDSWrapper.Audio;

public class NAudioWinBackend : IAudioBackend
{
    private readonly StreamingWaveProvider _waveProvider;
#if WINDOWS
    private readonly WaveOut _waveOut;
#endif

    public NAudioWinBackend(double sampleRate)
    {
        _waveProvider = new(new((int)sampleRate, 2));
#if WINDOWS
        _waveOut = new();
#endif
    }

    public void Initialize(double sampleRate)
    {
    }
    
    public bool ShouldPlay()
    {
#if WINDOWS
        return _waveOut.PlaybackState != PlaybackState.Playing;
#else
        return false;
#endif
    }

    public void Play()
    {
#if WINDOWS
        _waveOut.Play()
#endif
    }

    public void TogglePause()
    {
#if WINDOWS
        if (_waveOut.PlaybackState == PlaybackState.Playing)
        {
            _waveOut.Pause();
        }
        else
        {
            _waveOut.Play();
        }
#endif
    }

    public void AddSamples(byte[] samples)
    {
        _waveProvider.AddSamples(samples);
    }
}