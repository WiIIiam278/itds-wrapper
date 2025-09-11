namespace ITDSWrapper.Haptics;

public interface IHapticsBackend
{
    public void Initialize();
    public void Fire(bool press);
}