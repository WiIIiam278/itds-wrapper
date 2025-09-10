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
        InputButton.AddHandler(PointerPressedEvent, Button_OnPointerPressed, handledEventsToo: true);
        InputButton.AddHandler(PointerReleasedEvent, Button_OnPointerReleased, handledEventsToo: true);
    }

    private void Button_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ((VirtualButtonViewModel)DataContext!).Haptics.Fire();
        ((VirtualButtonViewModel)DataContext!).AssociatedInput?.Press((VirtualButtonViewModel)DataContext!);
    }

    private void Button_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ((VirtualButtonViewModel)DataContext!).Haptics.Fire();
        ((VirtualButtonViewModel)DataContext!).AssociatedInput?.Release((VirtualButtonViewModel)DataContext!);
    }
}