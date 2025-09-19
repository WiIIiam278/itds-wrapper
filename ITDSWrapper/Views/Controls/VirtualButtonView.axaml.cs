using Avalonia.Controls;
using ITDSWrapper.ViewModels.Controls;

namespace ITDSWrapper.Views.Controls;

public partial class VirtualButtonView : UserControl
{
    private bool _held;
    
    public VirtualButtonView()
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
        
        ((VirtualButtonViewModel)DataContext!).Haptics?.Fire(true);
        ((VirtualButtonViewModel)DataContext!).AssociatedInput?.Press((VirtualButtonViewModel)DataContext!);
        _held = true;
    }

    public void ReleaseButton()
    {
        if (!_held)
        {
            return;
        }
        
        ((VirtualButtonViewModel)DataContext!).Haptics?.Fire(false);
        ((VirtualButtonViewModel)DataContext!).AssociatedInput?.Release((VirtualButtonViewModel)DataContext!);
        _held = false;
    }
}