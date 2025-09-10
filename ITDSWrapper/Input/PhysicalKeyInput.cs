using Avalonia.Input;

namespace ITDSWrapper.Input;

public class PhysicalKeyInput(PhysicalKey physicalKey) : IGameInput<PhysicalKey>
{
    public bool IsSet { get; set; }
    
    private PhysicalKey _physicalKey = physicalKey;

    public void SetInput(PhysicalKey input)
    {
        _physicalKey = input;
    }

    public void Press(PhysicalKey input)
    {
        if (_physicalKey == input)
        {
            IsSet = true;
        }
    }

    public void Release(PhysicalKey input)
    {
        if (_physicalKey == input)
        {
            IsSet = false;
        }
    }
}