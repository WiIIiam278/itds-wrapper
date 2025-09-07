namespace ITDSWrapper.Input;

public class PointerState
{
    public bool Pressed { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public short RetroX => (short)((32767 * 2.0 + 1) / 256 * X - 0x7FFF);
    public short RetroY => (short)((32767 * 2.0 + 1) / 384 * Y - 0x7FFF);

    public void Press(double x, double y)
    {
        Pressed = y is > 192 and <= 384 && x is >= 0 and <= 256;
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