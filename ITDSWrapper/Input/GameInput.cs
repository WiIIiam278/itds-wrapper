using System;
using Avalonia.Input;

namespace ITDSWrapper.Input;

public class GameInput
{
    private InputType _inputType;
    private PhysicalKey _physicalKey;
    // private SteamInput
    
    public bool IsSet { get; private set; }

    public GameInput(PhysicalKey physicalKey)
    {
        _inputType = InputType.PHYSICAL_KEY;
        _physicalKey = physicalKey;
    }

    public GameInput()
    {
        _inputType = InputType.VIRTUAL_BUTTON;
    }

    public void SetPhysicalKey(PhysicalKey? physicalKey)
    {
        if (physicalKey is not null)
        {
            _inputType |= InputType.PHYSICAL_KEY;
            _physicalKey = (PhysicalKey)physicalKey;
        }
        else
        {
            _inputType &= ~InputType.PHYSICAL_KEY;
        }
    }

    public void PressPhysicalKey(PhysicalKey key)
    {
        if (_inputType.HasFlag(InputType.PHYSICAL_KEY) && key == _physicalKey)
        {
            IsSet = true;
        }
    }

    public void ReleasePhysicalKey(PhysicalKey key)
    {
        if (_inputType.HasFlag(InputType.PHYSICAL_KEY) && key == _physicalKey)
        {
            IsSet = false;
        }
    }

    public void PressVirtualButton()
    {
        if (_inputType.HasFlag(InputType.VIRTUAL_BUTTON))
        {
            IsSet = true;
        }
    }

    public void ReleaseVirtualButton()
    {
        if (_inputType.HasFlag(InputType.VIRTUAL_BUTTON))
        {
            IsSet = false;
        }
    }
}

[Flags]
public enum InputType
{
    PHYSICAL_KEY,
    STEAM_INPUT,
    VIRTUAL_BUTTON,
}