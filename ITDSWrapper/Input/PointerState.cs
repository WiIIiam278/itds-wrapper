namespace ITDSWrapper.Input;

public class PointerState(double width, double height)
{
    public bool Pressed { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = width;
    public double Height { get; set; } = height;
    public short RetroX => (short)((32767 * 2.0 + 1) / Width * X - 0x7FFF);
    public short RetroY => (short)((32767 * 2.0 + 1) / Height * Y - 0x7FFF);

    public void Press(double x, double y)
    {
        Pressed = y > Height / 2 && y <= Height && x >= 0 && x <= Width;
        if (Pressed)
        {
            X = x;
            Y = y;
        }
    }

    public void Release()
    {
        Pressed = false;
    }
}