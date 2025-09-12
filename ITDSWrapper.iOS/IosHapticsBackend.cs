using ITDSWrapper.Haptics;
using UIKit;

namespace ITDSWrapper.iOS;

public class IosHapticsBackend : IHapticsBackend
{
    private UIImpactFeedbackGenerator _generator;
    
    public void Initialize()
    {
        _generator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
    }

    public void Fire(bool press)
    {
        _generator.Prepare();
        _generator.ImpactOccurred();
    }
}