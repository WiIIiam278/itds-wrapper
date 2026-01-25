using Avalonia.Controls;
using ITDSWrapper.ViewModels.Controls;

namespace ITDSWrapper.Views.Controls;

public partial class VirtualButtonView : UserControl, IPressableButtonView
{
    private const int HoldTimerStart = 5;
    
    private bool _held;
    private int _holdTimer = HoldTimerStart;
    
    public VirtualButtonView()
    {
        Focusable = false;

        InitializeComponent();
    }
    
    public void PressButton(bool doHaptics)
    {
        if (_held)
        {
            _holdTimer = HoldTimerStart;
            return;
        }

        if (doHaptics)
        {
            ((VirtualButtonViewModel?)DataContext)?.Haptics?.Fire(true);
        }
        ((VirtualButtonViewModel?)DataContext)?.AssociatedInput?.Press((VirtualButtonViewModel)DataContext!);
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

        if (doHaptics)
        {
            ((VirtualButtonViewModel?)DataContext)?.Haptics?.Fire(false);
        }
        ((VirtualButtonViewModel?)DataContext)?.AssociatedInput?.Release((VirtualButtonViewModel)DataContext!);
        _held = false;
        _holdTimer = HoldTimerStart;
    }
}