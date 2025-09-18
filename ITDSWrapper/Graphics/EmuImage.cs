using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace ITDSWrapper.Graphics;

public class EmuImage : Control
{
    public static readonly AvaloniaProperty<EmuImageSource?> SourceProperty = AvaloniaProperty.Register<EmuImage, EmuImageSource?>(nameof(Source));

    public EmuImageSource? Source
    {
        get => (EmuImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    public override void Render(DrawingContext context)
    {
        Source?.Draw(context, new(0, 0, Width, Height), new(0, 0, Width, MaxHeight));
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }
}