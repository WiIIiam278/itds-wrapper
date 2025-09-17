using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Libretro.NET.Bindings;

namespace ITDSWrapper.Input;

public class DefaultInputDriver : IInputDriver
{
    private readonly Dictionary<uint, object?> _bindings;

    public DefaultInputDriver(bool isMobile, Action openSettings)
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
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_R2, null },
            };
        }
        else
        {
            _bindings = new()
            {
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_A, new PhysicalKeyInput(PhysicalKey.X) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_B, new PhysicalKeyInput(PhysicalKey.Z) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_X, new PhysicalKeyInput(PhysicalKey.S) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y, new PhysicalKeyInput(PhysicalKey.A) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_R, new PhysicalKeyInput(PhysicalKey.W) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_L, new PhysicalKeyInput(PhysicalKey.Q) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP, new PhysicalKeyInput(PhysicalKey.ArrowUp) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT, new PhysicalKeyInput(PhysicalKey.ArrowRight) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN, new PhysicalKeyInput(PhysicalKey.ArrowDown) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT, new PhysicalKeyInput(PhysicalKey.ArrowLeft) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_START, new PhysicalKeyInput(PhysicalKey.Enter) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT, new PhysicalKeyInput(PhysicalKey.ShiftRight) },
                { RetroBindings.RETRO_DEVICE_ID_JOYPAD_R2, new PhysicalKeyInput(PhysicalKey.Escape) { SpecialAction = openSettings } },
            };
        }
    }

    public void Shutdown()
    {
    }

    public uint[] GetInputKeys()
    {
        return _bindings.Keys.ToArray();
    }

    public void SetActionSet(string actionSet)
    {
    }


    public void SetBinding<T>(uint input, IGameInput<T>? binding)
    {
        if (!_bindings.TryAdd(input, binding))
        {
            _bindings[input] = binding;
        }
    }

    public bool QueryInput(uint id)
    {
        return _bindings.ContainsKey(id) && _bindings[id] is IGameInputSettable { IsSet: true };
    }

    public void Push<T>(T binding)
    {
        foreach (IGameInput<T>? input in _bindings.Values.Where(i => i is IGameInput<T>))
        {
            input?.Press(binding);
        }
    }

    public void Release<T>(T binding)
    {
        foreach (IGameInput<T>? input in _bindings.Values.Where(i => i is IGameInput<T>))
        {
            input?.Release(binding);
        }
    }

    public void DoRumble(ushort strength)
    {
    }
}