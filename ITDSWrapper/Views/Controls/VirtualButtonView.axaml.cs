using Avalonia.Controls;
using Avalonia.Input;
using ITDSWrapper.ViewModels.Controls;

namespace ITDSWrapper.Views.Controls;

public partial class VirtualButtonView : UserControl
{
    public VirtualButtonView()
    {
        Focusable = false;

        InitializeComponent();
        InputButton.AddHandler(PointerPressedEvent, Button_OnPointerEntered, handledEventsToo: true);
        InputButton.AddHandler(PointerReleasedEvent, Button_OnPointerExited, handledEventsToo: true);
    }

    private void Button_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        ((VirtualButtonViewModel)DataContext!).Haptics?.Fire(true);
        ((VirtualButtonViewModel)DataContext!).AssociatedInput?.Press((VirtualButtonViewModel)DataContext!);
    }
    
    private void Button_OnPointerExited(object? sender, PointerEventArgs e)
    {
        ((VirtualButtonViewModel)DataContext!).Haptics?.Fire(false);
        ((VirtualButtonViewModel)DataContext!).AssociatedInput?.Release((VirtualButtonViewModel)DataContext!);
    }

}