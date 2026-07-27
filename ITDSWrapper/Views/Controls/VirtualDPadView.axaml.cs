using Avalonia.Controls;
using Avalonia.Input;
using ITDSWrapper.Input;
using ITDSWrapper.ViewModels.Controls;

namespace ITDSWrapper.Views.Controls;

public partial class VirtualDPadView : UserControl
{
    public VirtualDPadView()
    {
        InitializeComponent();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Pointer.Capture(this);
        
        ((VirtualDPadViewModel)DataContext!).Update(e.Pointer.Id, e.GetCurrentPoint(this));
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ((VirtualDPadViewModel)DataContext!).Update(e.Pointer.Id, e.GetCurrentPoint(this));
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ((VirtualDPadViewModel)DataContext!).Release(e.Pointer.Id);
        base.OnPointerReleased(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ((VirtualDPadViewModel)DataContext!).Release(e.Pointer.Id);
        base.OnPointerCaptureLost(e);
    }
}