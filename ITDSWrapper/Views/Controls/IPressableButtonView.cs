using Avalonia;

namespace ITDSWrapper.Views.Controls;

public interface IPressableButtonView
{
    public Rect Bounds { get; }
    public void PressButton(bool doHaptics = false);
    public void ReleaseButton(bool doHaptics = true, bool softRelease = false);
}