using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Libretro.NET.Bindings;

namespace ITDSWrapper.Input;

public class InputBindings
{
    private readonly Dictionary<uint, GameInput?> _bindings;

    public InputBindings(bool isMobile)
    {
        if (isMobile)
        {
            _bindings = new()
            {
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_A, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_B, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_X, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_R, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_L, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_START, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT, null },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_R3, null },
            };
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
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_R3, new(PhysicalKey.Escape) },
            };
        }
    }

    public uint[] GetInputKeys()
    {
        return _bindings.Keys.ToArray();
    }

    public void SetBinding(uint input, GameInput? binding)
    {
        if (!_bindings.TryAdd(input, binding))
        {
            _bindings[input] = binding;
        }
    }

    public bool QueryInput(uint id)
    {
        return _bindings.ContainsKey(id) && (_bindings[id]?.IsSet ?? false);
    }

    public void PushKey(PhysicalKey key)
    {
        foreach (GameInput? input in _bindings.Values)
        {
            input?.PressPhysicalKey(key);
        }
    }

    public void ReleaseKey(PhysicalKey key)
    {
        foreach (GameInput? input in _bindings.Values)
        {
            input?.ReleasePhysicalKey(key);
        }
    }
    
}