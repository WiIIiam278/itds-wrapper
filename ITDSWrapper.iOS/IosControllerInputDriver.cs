using System.Collections.Generic;
using System.Linq;
using CoreHaptics;
using GameController;
using ITDSWrapper.Input;
using Libretro.NET.Bindings;

namespace ITDSWrapper.iOS;

public class IosControllerInputDriver : IInputDriver
{
    public GCController? Controller { get; private set; }
    private CHHapticEngine? _hapticEngine;
    private readonly Dictionary<uint, IosControllerInput?> _actionsDictionary = [];

    public void SetController(GCController controller)
    {
        _hapticEngine = Controller?.Haptics?.CreateEngine(GCHapticsLocality.Default);
        Controller = controller;
        _actionsDictionary.Clear();
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_A, new(Controller.ExtendedGamepad?.ButtonA));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_B, new(Controller.ExtendedGamepad?.ButtonB));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_X, new(Controller.ExtendedGamepad?.ButtonX));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y, new(Controller.ExtendedGamepad?.ButtonY));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP, new(Controller.ExtendedGamepad?.DPad.Up));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT, new(Controller.ExtendedGamepad?.DPad.Right));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN, new(Controller.ExtendedGamepad?.DPad.Down));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT, new(Controller.ExtendedGamepad?.DPad.Left));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_L, new(Controller.ExtendedGamepad?.LeftShoulder));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_R, new(Controller.ExtendedGamepad?.RightShoulder));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_START, new(Controller.ExtendedGamepad?.ButtonMenu));
        _actionsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT, new(Controller.ExtendedGamepad?.ButtonOptions));
    }
    
    public void Shutdown()
    {
    }

    public uint[] GetInputKeys()
    {
        return _actionsDictionary.Keys.ToArray();
    }

    public void SetActionSet(string actionSet)
    {
    }

    public void SetBinding<T>(uint input, IGameInput<T>? binding)
    {
        if (binding is IosControllerInput iosBinding)
        {
            _actionsDictionary[input] = iosBinding;
        }

        if (binding is null)
        {
            _actionsDictionary[input] = null;
        }
    }

    public bool QueryInput(uint id)
    {
        return _actionsDictionary.ContainsKey(id) && (_actionsDictionary[id]?.IsSet ?? false);
    }

    public void Push<T>(T binding)
    {
        if (binding is GCControllerElement iosBinding)
        {
            foreach (IosControllerInput? input in _actionsDictionary.Values)
            {
                input?.Press(iosBinding);
            }
        }
    }

    public void Release<T>(T binding)
    {
        if (binding is GCControllerElement iosBinding)
        {
            foreach (IosControllerInput? input in _actionsDictionary.Values)
            {
                input?.Release(iosBinding);
            }
        }
    }

    public void DoRumble(ushort strength)
    {
        // TODO: Implement haptic rumble
    }
}