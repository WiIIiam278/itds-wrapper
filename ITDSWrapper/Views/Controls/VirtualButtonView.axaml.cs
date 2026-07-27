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
        AddHandler(PointerPressedEvent, (_, _) => PressButton(), handledEventsToo: true);
        AddHandler(PointerReleasedEvent, (_, _) => ReleaseButton(), handledEventsToo: true);
        InputButton.AddHandler(PointerCaptureLostEvent, (_, _) => ReleaseButton(), handledEventsToo: true);
    }

    private void PressButton()
    {
        if (_held || DataContext is not VirtualButtonViewModel ctx)
        {
            return;
        }

        if (ctx.HapticsEnabled)
        {
            ctx.Haptics?.Fire(true);
        }
        ctx.AssociatedInput?.Press(ctx);
        _held = true;
    }

    private void ReleaseButton()
    {
        if (!_held || DataContext is not VirtualButtonViewModel ctx)
        {
            return;
        }

        if (ctx.HapticsEnabled)
        {
            ctx.Haptics?.Fire(false);
        }
        ctx.AssociatedInput?.Release(ctx);
        _held = false;
    }
}
