using Avalonia.Input;

namespace ITDSWrapper.Input;

public class AvaloniaPointerState(double x, double y)
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;

    public void Update(PointerPoint point)
    {
        X = point.Position.X;
        Y = point.Position.Y;
    }
}