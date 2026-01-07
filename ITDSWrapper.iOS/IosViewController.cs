namespace ITDSWrapper.iOS;

using UIKit;
using Avalonia.iOS;

public class IosViewController : DefaultAvaloniaViewController
{
    public override UIRectEdge PreferredScreenEdgesDeferringSystemGestures => UIRectEdge.Bottom;
    public override bool PrefersHomeIndicatorAutoHidden => true;
}