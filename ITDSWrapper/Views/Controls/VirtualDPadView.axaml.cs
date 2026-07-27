using Avalonia.Controls;
using Avalonia.Input;
using ITDSWrapper.ViewModels.Controls;

namespace ITDSWrapper.Views.Controls;

public partial class VirtualDPadView : UserControl
{
    public VirtualDPadView()
    {
        InitializeComponent();
        
        AddHandler(PointerPressedEvent, (_, e) => Pressed(e), handledEventsToo: true);
        AddHandler(PointerReleasedEvent, (_, e) => Released(e), handledEventsToo: true);
    }

    private void Pressed(PointerPressedEventArgs e)
    {
        e.Pointer.Capture(this);
        
        ((VirtualDPadViewModel)DataContext!).Update(e.Pointer.Id, e.GetCurrentPoint(this));
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ((VirtualDPadViewModel)DataContext!).Update(e.Pointer.Id, e.GetCurrentPoint(this));
        base.OnPointerMoved(e);
    }

    private void Released(PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        
        ((VirtualDPadViewModel)DataContext!).Release(e.Pointer.Id);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ((VirtualDPadViewModel)DataContext!).Release(e.Pointer.Id);
        base.OnPointerCaptureLost(e);
    }
}