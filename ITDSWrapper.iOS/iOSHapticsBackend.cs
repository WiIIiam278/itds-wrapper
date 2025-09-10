using ITDSWrapper.Haptics;
using UIKit;

namespace ITDSWrapper.iOS;

public class iOSHapticsBackend : IHapticsBackend
{
    private UIImpactFeedbackGenerator _generator;
    
    public void Initialize()
    {
        _generator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
    }

    public void Fire()
    {
        _generator.Prepare();
        _generator.ImpactOccurred();
    }
}