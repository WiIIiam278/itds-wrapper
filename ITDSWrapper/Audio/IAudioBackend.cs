namespace ITDSWrapper.Audio;

public interface IAudioBackend
{
    public void Initialize(double sampleRate);
    public bool ShouldPlay();
    public void Play();
    public void TogglePause();
    public void AddSamples(byte[] samples);
}