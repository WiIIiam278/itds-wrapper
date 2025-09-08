using ITDSWrapper.Audio;
#if WINDOWS
using NAudio.Wave;
#endif

namespace ITDSWrapper.Desktop;
public class NAudioWinBackend : IAudioBackend
{
    private StreamingWaveProvider _waveProvider;
#if WINDOWS
    private readonly WaveOut _waveOut;
#endif

    public NAudioWinBackend()
    {
#if WINDOWS
        _waveOut = new();
#endif
    }

    public void Initialize(double sampleRate)
    {
#if WINDOWS
        _waveProvider = new(new((int)sampleRate, 2));
        _waveOut.Init(_waveProvider);
#endif
    }
    
    public bool ShouldStart()
    {
#if WINDOWS
        return _waveOut.PlaybackState != PlaybackState.Playing;
#else
        return false;
#endif
    }

    public bool ShouldHoldEmulation()
    {
        return false;
    }

    public void Play()
    {
#if WINDOWS
        _waveOut.Play();
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

    public void PlaySamples(byte[] samples)
    {
        _waveProvider.AddSamples(samples);
    }
}