using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace ITDSWrapper.Controls;

public class EmuImage : IImage, IDisposable
{
    private byte[] _frame;
    private EmuDrawOperation? _drawOperation;
    public Size Size { get; }

    public EmuImage(byte[] frame, uint width, uint height)
    {
        _frame = frame;
        Size = new(width, height);
    }

    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        _drawOperation ??= new()
        {
            Frame = _frame,
            Width = (int)Size.Width,
            Height = (int)Size.Height,
            Bounds = destRect,
        };
        context.Custom(_drawOperation);
    }

    public void SetFrame(byte[] frame)
    {
        _frame = frame;
    }

    public void Dispose()
    {
        _drawOperation?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class EmuDrawOperation : ICustomDrawOperation
{
    public Rect Bounds { get; set; }
    public required byte[] Frame { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public bool Equals(ICustomDrawOperation? other) => false;

    public void Dispose()
    {
    }

    public bool HitTest(Point p) => Bounds.Contains(p);

    public void Render(ImmediateDrawingContext context)
    {
        if (context.PlatformImpl.GetFeature<ISkiaSharpApiLeaseFeature>() is { } leaseFeature)
        {
            ISkiaSharpApiLease lease = leaseFeature.Lease();
            using (lease)
            {
                GCHandle handle = GCHandle.Alloc(Frame, GCHandleType.Pinned);

                SKPixmap pixmap = new(new(Width, Height, SKColorType.Bgra8888), handle.AddrOfPinnedObject());
                lease.SkCanvas.DrawImage(SKImage.FromPixels(pixmap), 0, 0);
                
                handle.Free();
            }
        }
    }
}