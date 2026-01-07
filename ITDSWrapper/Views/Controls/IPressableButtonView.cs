using Avalonia;

namespace ITDSWrapper.Views.Controls;

public interface IPressableButtonView
{
    public Rect Bounds { get; }
    public void PressButton();
    public void ReleaseButton(bool softRelease = false);
}