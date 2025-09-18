using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace ITDSWrapper.Graphics;

public class EmuImageSource : IImage, IDisposable
{
    private byte[] _frame;
    private EmuDrawOperation? _drawOperation;
    public Size Size { get; }

    public EmuImageSource(byte[] frame, uint width, uint height)
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
        _drawOperation.Bounds = destRect;
        context.Custom(_drawOperation);
    }

    public void SetFrame(byte[] frame)
    {
        Array.Copy(frame, _frame, _frame.Length);
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
                lease.SkCanvas.DrawImage(SKImage.FromPixels(pixmap), new SKRect(0, 0, Width, Height),
                    new SKRect((float)Bounds.X, (float)Bounds.Y, (float)(Bounds.X + Bounds.Width), (float)(Bounds.Y + Bounds.Height)),
                    new() { IsAntialias = true});
                
                handle.Free();
            }
        }
    }
}