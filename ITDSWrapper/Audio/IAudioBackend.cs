namespace ITDSWrapper.Audio;

public interface IAudioBackend
{
    public bool ShouldPlay();
    public void Play();
    public void AddSamples(byte[] samples);
}