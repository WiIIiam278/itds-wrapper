using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Views;
using ITDSWrapper.Input;
using Libretro.NET.Bindings;

namespace ITDSWrapper.Android;

public class AndroidControllerInputDriver : IInputDriver
{
    public InputDevice? Controller { get; set; }
    private readonly Dictionary<uint, AndroidControllerInput?> _inputDictionary = [];

    public void SetController(InputDevice controller)
    {
        Controller = controller;
        _inputDictionary.Clear();
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_A, new(new(AndroidInputType.KEY, Keycode.ButtonA, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_B, new(new(AndroidInputType.KEY, Keycode.ButtonB, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_X, new(new(AndroidInputType.KEY, Keycode.ButtonX, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y, new(new(AndroidInputType.KEY, Keycode.ButtonY, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP, new(new(AndroidInputType.KEY, Keycode.DpadUp, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT, new(new(AndroidInputType.KEY, Keycode.DpadRight, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN, new(new(AndroidInputType.KEY, Keycode.DpadDown, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT, new(new(AndroidInputType.KEY, Keycode.DpadLeft, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_L, new(new(AndroidInputType.KEY, Keycode.ButtonL1, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_R, new(new(AndroidInputType.KEY, Keycode.ButtonR1, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_START, new(new(AndroidInputType.KEY, Keycode.ButtonStart, null)));
        _inputDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT, new(new(AndroidInputType.KEY, Keycode.ButtonSelect, null)));
    }
    
    public void Shutdown()
    {
    }

    public uint[] GetInputKeys()
    {
        return _inputDictionary.Keys.ToArray();
    }

    public void SetActionSet(string actionSet)
    {
    }

    public void SetBinding<T>(uint input, IGameInput<T>? binding)
    {
        if (binding is AndroidControllerInput androidBinding)
        {
            _inputDictionary[input] = androidBinding;
        }

        if (binding is null)
        {
            _inputDictionary[input] = null;
        }
    }

    public bool QueryInput(uint id)
    {
        return _inputDictionary.ContainsKey(id) && (_inputDictionary[id]?.IsSet ?? false);
    }

    public void Push<T>(T binding)
    {
        if (binding is AndroidInputContainer androidInput)
        {
            foreach (AndroidControllerInput? input in _inputDictionary.Values)
            {
                input?.Press(androidInput);
            }
        }
    }

    public void Release<T>(T binding)
    {
        lock (_inputDictionary)
        {
            if (binding is AndroidInputContainer androidInput)
            {
                foreach (AndroidControllerInput? input in _inputDictionary.Values)
                {
                    input?.Release(androidInput);
                }
            }
        }
    }

    public void DoRumble(ushort strength)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            Controller?.VibratorManager.DefaultVibrator.Vibrate(VibrationEffect.CreateOneShot(1000, strength / 258 + 1));
        }
        else
        {
            Controller?.Vibrator?.Vibrate(VibrationEffect.CreateOneShot(1000, strength / 258 + 1));
        }
    }
}