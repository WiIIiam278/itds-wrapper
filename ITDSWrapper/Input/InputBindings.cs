using System;
using System.Collections.Generic;
using Avalonia.Input;
using Libretro.NET.Bindings;

namespace ITDSWrapper.Input;

public class InputBindings
{
    private Dictionary<uint, PhysicalInput> _bindings;

    public InputBindings()
    {
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            _bindings = [];
        }
        else
        {
            _bindings = new()
            {
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_A, new(PhysicalKey.X) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_B, new(PhysicalKey.Z) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_X, new(PhysicalKey.S) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y, new(PhysicalKey.A) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_R, new(PhysicalKey.W) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_L, new(PhysicalKey.Q) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP, new(PhysicalKey.ArrowUp) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT, new(PhysicalKey.ArrowRight) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN, new(PhysicalKey.ArrowDown) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT, new(PhysicalKey.ArrowLeft) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_START, new(PhysicalKey.Enter) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT, new(PhysicalKey.ShiftRight) },
            };
        }
    }

    public bool QueryInput(uint id)
    {
        return _bindings.ContainsKey(id) && _bindings[id].IsSet;
    }

    public void PushKey(PhysicalKey key)
    {
        foreach (PhysicalInput input in _bindings.Values)
        {
            input.PressPhysicalKey(key);
        }
    }

    public void ReleaseKey(PhysicalKey key)
    {
        foreach (PhysicalInput input in _bindings.Values)
        {
            input.ReleasePhysicalKey(key);
        }
    }
}

public class PhysicalInput
{
    private InputType _inputType;
    private PhysicalKey _physicalKey;
    
    public bool IsSet { get; private set; }

    public PhysicalInput(PhysicalKey physicalKey)
    {
        _inputType = InputType.PHYSICAL_KEY;
        _physicalKey = physicalKey;
    }

    public void PressPhysicalKey(PhysicalKey key)
    {
        if (_inputType == InputType.PHYSICAL_KEY && key == _physicalKey)
        {
            IsSet = true;
        }
    }

    public void ReleasePhysicalKey(PhysicalKey key)
    {
        if (_inputType == InputType.PHYSICAL_KEY && key == _physicalKey)
        {
            IsSet = false;
        }
    }

    public void PressVirtualButton()
    {
        if (_inputType == InputType.VIRTUAL_BUTTON)
        {
            IsSet = true;
        }
    }

    public void ReleaseVirtualButton()
    {
        if (_inputType == InputType.VIRTUAL_BUTTON)
        {
            IsSet = false;
        }
    }
}

public enum InputType
{
    PHYSICAL_KEY,
    STEAM_INPUT,
    VIRTUAL_BUTTON,
}