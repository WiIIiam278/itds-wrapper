using Avalonia.Controls;
using ITDSWrapper.ViewModels.Controls;

namespace ITDSWrapper.Views.Controls;

public partial class VirtualMultiButtonView : UserControl, IPressableButtonView
{
    private const int HoldTimerStart = 5;

    private bool _held;
    private int _holdTimer = HoldTimerStart;

    public VirtualMultiButtonView()
    {
        Focusable = false;

        InitializeComponent();
    }

    public void PressButton(bool doHaptics = true)
    {
        if (_held)
        {
            _holdTimer = HoldTimerStart;
            return;
        }

        var ctx = (VirtualMultiButtonViewModel)DataContext!;
        if (doHaptics)
        {
            ctx.Haptics?.Fire(false);
        }
        foreach (var button in ctx.Buttons)
        {
            button.AssociatedInput?.Press(button);
        }
        _held = true;
        _holdTimer = HoldTimerStart;
    }

    public void ReleaseButton(bool doHaptics = true, bool softRelease = false)
    {
        if (!_held)
        {
            return;
        }

        if (softRelease && _holdTimer > 0)
        {
            _holdTimer--;
            return;
        }

        var ctx = (VirtualMultiButtonViewModel)DataContext!;
        if (doHaptics)
        {
            ctx.Haptics?.Fire(false);
        }
        foreach (var button in ctx.Buttons)
        {
            button.AssociatedInput?.Release(button);
        }
        _held = false;
        _holdTimer = HoldTimerStart;
    }
}