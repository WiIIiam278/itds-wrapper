using System;
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


/*
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace ITDSWrapper.Graphics;

public class EmuImage : Control
{
    public static readonly AvaloniaProperty<byte[]?> SourceProperty = AvaloniaProperty.Register<EmuImage, byte[]?>(nameof(Source));

    private EmuDrawOperation? _drawOperation;

    public byte[]? Source
    {
        get => (byte[]?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        if (Source is null)
        {
            return;
        }
        _drawOperation ??= new()
        {
            Frame = Source,
            Width = 256,
            Height = 384,
            Bounds = new(0, 0, Width, Height),
        };
        context.Custom(_drawOperation);
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }
}*/