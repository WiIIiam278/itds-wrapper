using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ITDSWrapper.Input;
using Libretro.NET.Bindings;
using Silk.NET.Input;
using Silk.NET.Input.Sdl;

namespace ITDSWrapper.Desktop;

public class SdlInputDriver : IInputDriver
{
    private readonly SdlInputContextHost _contextHost;
    private IInputContext? _inputContext;
    private IGamepad? _gamepad;
    private Action<IGamepad, Button>? _buttonDownHandler;
    private Action<IGamepad, Button>? _buttonUpHandler;
    private Action<IGamepad, Thumbstick>? _thumbstickMovedHandler;
    private Action<IGamepad, Trigger>? _triggerMovedHandler;

    private readonly Dictionary<uint, SdlControllerInput?> _controlsDictionary = [];

    private static readonly Dictionary<string, uint> ButtonNamesMap = new()
    {
        { "A", RetroBindings.RETRO_DEVICE_ID_JOYPAD_A },
        { "B", RetroBindings.RETRO_DEVICE_ID_JOYPAD_B },
        { "X", RetroBindings.RETRO_DEVICE_ID_JOYPAD_X },
        { "Y", RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y },
        { "L", RetroBindings.RETRO_DEVICE_ID_JOYPAD_L },
        { "R", RetroBindings.RETRO_DEVICE_ID_JOYPAD_R },
        { "DPad", RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP },
        { "Start", RetroBindings.RETRO_DEVICE_ID_JOYPAD_START },
        { "Select", RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT },
    };

    public bool RequestInputUpdate { get; set; }

    public SdlInputDriver(SdlInputContextHost contextHost)
    {
        SdlInput.RegisterPlatform();
        _contextHost = contextHost;
    }

    public bool HasInputContext => _inputContext is not null;

    public void SetInputContext()
    {
        if (_contextHost.View?.IsInitialized != true)
            return;

        _inputContext = _contextHost.View?.CreateInput();
        if (HasInputContext)
        {
            _inputContext!.ConnectionChanged += (device, _) =>
            {
                if (device is IGamepad { IsConnected: true } gamepad)
                {
                    SetGamepad(gamepad);
                }
            };
            Dispatcher.UIThread.Invoke(() => _contextHost.View?.DoEvents());
            if (_inputContext.Gamepads.Count > 0)
            {
                SetGamepad(_inputContext.Gamepads[0]);
            }
        }
    }

    public void PumpView()
    {
        Dispatcher.UIThread.Invoke(() => _contextHost.View?.DoEvents());
    }

    public void SetGamepad(IGamepad? gamepad)
    {
        if (_gamepad is not null)
        {
            if (_buttonDownHandler is not null) _gamepad.ButtonDown -= _buttonDownHandler;
            if (_buttonUpHandler is not null) _gamepad.ButtonUp -= _buttonUpHandler;
            if (_thumbstickMovedHandler is not null) _gamepad.ThumbstickMoved -= _thumbstickMovedHandler;
            if (_triggerMovedHandler is not null) _gamepad.TriggerMoved -= _triggerMovedHandler;
        }

        _gamepad = gamepad;
        _controlsDictionary.Clear();
        _buttonDownHandler = null;
        _buttonUpHandler = null;
        _thumbstickMovedHandler = null;
        _triggerMovedHandler = null;

        if (_gamepad is null)
            return;

        foreach (Button button in _gamepad.Buttons)
        {
            switch (button.Name)
            {
                case ButtonName.A:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_A, new(button));
                    break;
                case ButtonName.B:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_B, new(button));
                    break;
                case ButtonName.X:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_X, new(button));
                    break;
                case ButtonName.Y:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y, new(button));
                    break;
                case ButtonName.LeftBumper:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_L, new(button));
                    break;
                case ButtonName.RightBumper:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_R, new(button));
                    break;
                case ButtonName.DPadUp:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP, new(button));
                    break;
                case ButtonName.DPadRight:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT, new(button));
                    break;
                case ButtonName.DPadDown:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN, new(button));
                    break;
                case ButtonName.DPadLeft:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT, new(button));
                    break;
                case ButtonName.Start:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_START, new(button));
                    break;
                case ButtonName.Back:
                    _controlsDictionary.Add(RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT, new(button));
                    break;
            }
        }

        if (_gamepad?.Thumbsticks.Count > 0)
        {
            _controlsDictionary.TryAdd(RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP,
                new((_gamepad.Thumbsticks[0], ThumbstickDirection.NEGATIVE_Y)));
            _controlsDictionary.TryAdd(RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT,
                new((_gamepad.Thumbsticks[0], ThumbstickDirection.POSITIVE_X)));
            _controlsDictionary.TryAdd(RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN,
                new((_gamepad.Thumbsticks[0], ThumbstickDirection.POSITIVE_Y)));
            _controlsDictionary.TryAdd(RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT,
                new((_gamepad.Thumbsticks[0], ThumbstickDirection.NEGATIVE_X)));
        }

        _buttonDownHandler = (_, button) => Push(button);
        _buttonUpHandler = (_, button) => Release(button);
        _thumbstickMovedHandler = (_, thumbstick) =>
        {
            Push(thumbstick);
            Release(thumbstick);
        };
        _triggerMovedHandler = (_, trigger) =>
        {
            Push(trigger);
            Release(trigger);
        };

        _gamepad!.ButtonDown += _buttonDownHandler;
        _gamepad.ButtonUp += _buttonUpHandler;
        _gamepad.ThumbstickMoved += _thumbstickMovedHandler;
        _gamepad.TriggerMoved += _triggerMovedHandler;
    }

    public void Shutdown()
    {
    }

    public uint[] GetInputKeys()
    {
        return _controlsDictionary.Keys.ToArray();
    }

    public void SetActionSet(string actionSet)
    {
    }

    public void SetBinding<T>(uint input, IGameInput<T>? binding)
    {
        _controlsDictionary[input] = binding switch
        {
            SdlControllerInput sdlInput => sdlInput,
            null => null,
            _ => _controlsDictionary[input]
        };
    }

    public bool QueryInput(uint id)
    {
        return _controlsDictionary.ContainsKey(id) && (_controlsDictionary[id]?.IsSet ?? false);
    }

    public void Push<T>(T binding)
    {
        foreach (SdlControllerInput? input in _controlsDictionary.Values)
        {
            input?.Press(binding);
        }
    }

    public void Release<T>(T binding)
    {
        foreach (SdlControllerInput? input in _controlsDictionary.Values)
        {
            input?.Release(binding);
        }
    }

    public void DoRumble(ushort strength)
    {
        foreach (IMotor motor in _gamepad?.VibrationMotors ?? [])
        {
            motor.Speed = (float)strength / ushort.MaxValue;

            Task.Run(StopMotor);
            continue;

            async Task StopMotor()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                motor.Speed = 0;
            }
        }
    }

    public uint[] GetActionGlyphId(string button)
    {
        return [];
    }
}