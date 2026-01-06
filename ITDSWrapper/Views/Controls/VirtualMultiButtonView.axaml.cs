using Avalonia.Controls;
using ITDSWrapper.ViewModels.Controls;

namespace ITDSWrapper.Views.Controls;

public partial class VirtualMultiButtonView : UserControl
{
    private bool _held;
    
    public VirtualMultiButtonView()
    {
        Focusable = false;

        InitializeComponent();
    }
    
    public void PressButton()
    {
        if (_held)
        {
            return;
        }

        ((VirtualMultiButtonViewModel) DataContext!).Haptics?.Fire(true);
        foreach (var button in ((VirtualMultiButtonViewModel) DataContext!).Buttons)
        {
            button.AssociatedInput?.Press((VirtualButtonViewModel)DataContext!);
        }
        _held = true;
    }

    public void ReleaseButton()
    {
        if (!_held)
        {
            return;
        }

        ((VirtualMultiButtonViewModel) DataContext!).Haptics?.Fire(false);
        foreach (var button in ((VirtualMultiButtonViewModel) DataContext!).Buttons)
        {
            button.AssociatedInput?.Release((VirtualButtonViewModel)DataContext!);
        }
        _held = false;
    }
}