using System;
using System.Collections.Generic;
using System.Linq;
using ITDSWrapper.Input;
using Libretro.NET.Bindings;
using Steamworks;

namespace ITDSWrapper.Desktop.Steam;

public class SteamInputDriver : IInputDriver
{
    private Controller _controller;
    private readonly Dictionary<uint, SteamControllerInput?> _actionsDictionary = [];

    private static readonly Dictionary<string, SteamInputAction[]> ActionSets = new()
    {
        {
            "OverworldControls",
            [
                new("Move",
                [
                    RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT, RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN,
                    RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT, RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP
                ]),
                new("interact", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_A]),
                new("cancel", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_B]),
                new("pause_menu", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_START]),
                new("debug_menu", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT]),
            ]
        },
        {
            "BattleControls",
            [
                new("menu_up", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP]),
                new("menu_down", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN]),
                new("menu_left", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT]),
                new("menu_right", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT]),
                new("confirm", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_A]),
                new("back", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_B]),
                new("start_move", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y]),
                new("attack", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_X]),
                new("target_right", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_R]),
                new("target_left", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_L]),
            ]
        },
        {
            "ScriptControls",
            [
                new("advance", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_A]),
                new("cancel", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_B]),
                new("auto", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y]),
                new("fast_forward", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_R]),
                new("menu_up", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP]),
                new("menu_down", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN]),
                new("menu_left", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT]),
                new("menu_right", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT]),
            ]
        },
        {
            "MenuControls",
            [
                new("menu_up", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP]),
                new("menu_down", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN]),
                new("menu_left", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT]),
                new("menu_right", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT]),
                new("menu_select", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_A]),
                new("cancel", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_B]),
                new("pause_menu", [RetroBindings.RETRO_DEVICE_ID_JOYPAD_START]),
            ]
        },
    };

    private static readonly Dictionary<string, uint> ButtonNamesMap = new()
    {
        { "A", RetroBindings.RETRO_DEVICE_ID_JOYPAD_A },
        { "B", RetroBindings.RETRO_DEVICE_ID_JOYPAD_B },
        { "X", RetroBindings.RETRO_DEVICE_ID_JOYPAD_X },
        { "Y", RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y },
        { "L", RetroBindings.RETRO_DEVICE_ID_JOYPAD_L },
        { "R", RetroBindings.RETRO_DEVICE_ID_JOYPAD_R },
        { "DPad", RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT },
    };

    private string? _currentActionSet;

    public SteamInputDriver()
    {
        SteamInput.Init();
    }

    public void SetController(Controller controller)
    {
        _controller = controller;
    }

    public bool UpdateState()
    {
        bool controllerUsed = false;
        if (!string.IsNullOrEmpty(_currentActionSet))
        {
            _controller.ActionSet = _currentActionSet;
            foreach (SteamInputAction input in ActionSets[_currentActionSet])
            {
                if (input.RetroBindings.Length > 1)
                {
                    AnalogState state = _controller.GetAnalogState(input.ActionName);
                    if (state.X > 0.05f)
                    {
                        controllerUsed = true;
                        Push($"{input.ActionName}_RIGHT");
                        Release($"{input.ActionName}_LEFT");
                    }
                    else if (state.X < -0.05f)
                    {
                        controllerUsed = true;
                        Release($"{input.ActionName}_RIGHT");
                        Push($"{input.ActionName}_LEFT");
                    }
                    else
                    {
                        Release($"{input.ActionName}_RIGHT");
                        Release($"{input.ActionName}_LEFT");
                    }

                    if (state.Y < -0.05f)
                    {
                        controllerUsed = true;
                        Push($"{input.ActionName}_DOWN");
                        Release($"{input.ActionName}_UP");
                    }
                    else if (state.Y > 0.05f)
                    {
                        controllerUsed = true;
                        Release($"{input.ActionName}_DOWN");
                        Push($"{input.ActionName}_UP");
                    }
                    else
                    {
                        Release($"{input.ActionName}_DOWN");
                        Release($"{input.ActionName}_UP");
                    }
                }
                else
                {
                    if (_controller.GetDigitalState(input.ActionName).Pressed)
                    {
                        controllerUsed = true;
                        Push(input.ActionName);
                    }
                    else
                    {
                        Release(input.ActionName);
                    }
                }
            }
        }

        return controllerUsed;
    }

    public void Shutdown()
    {
        SteamInput.Shutdown();
    }

    public uint[] GetInputKeys()
    {
        return _actionsDictionary.Keys.ToArray();
    }

    public void SetActionSet(string actionSet)
    {
        _currentActionSet = actionSet;

        _actionsDictionary.Clear();
        foreach (SteamInputAction input in ActionSets[_currentActionSet])
        {
            if (input.RetroBindings.Length > 1)
            {
                string[] directions = ["RIGHT", "DOWN", "LEFT", "UP"];
                for (int i = 0; i < input.RetroBindings.Length; i++)
                {
                    _actionsDictionary.Add(input.RetroBindings[i], new($"{input.ActionName}_{directions[i]}"));
                }
            }
            else
            {
                _actionsDictionary.Add(input.RetroBindings[0], new(input.ActionName));
            }
        }
    }

    public void SetBinding<T>(uint input, IGameInput<T>? binding)
    {
        if (binding is SteamControllerInput action)
        {
            _actionsDictionary[input] = action;
        }
        else if (binding is null)
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
        if (binding is string action)
        {
            foreach (SteamControllerInput? input in _actionsDictionary.Values)
            {
                input?.Press(action);
            }
        }
    }

    public void Release<T>(T binding)
    {
        if (binding is string action)
        {
            foreach (SteamControllerInput? input in _actionsDictionary.Values)
            {
                input?.Release(action);
            }
        }
    }

    public void DoRumble(ushort strength)
    {
        _controller.TriggerVibration(strength, strength);
    }

    public uint GetActionGlyphId(string button)
    {
        if (string.IsNullOrEmpty(_currentActionSet))
        {
            return ButtonNamesMap[button];
        }

        string? actionName = ActionSets[_currentActionSet]
            .FirstOrDefault(a =>
                a.RetroBindings.Contains(ButtonNamesMap[button] == RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT
                    ? RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP
                    : ButtonNamesMap[button]))?.ActionName;
        if (string.IsNullOrEmpty(actionName))
        {
            return ButtonNamesMap[button];
        }

        string glyph = SteamInput.GetSvgActionGlyph(_controller, actionName);
        if (string.IsNullOrEmpty(glyph))
        {
            return ButtonNamesMap[button];
        }

        if (glyph.EndsWith("button_a.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_A;
        }

        if (glyph.EndsWith("button_b.svg") && glyph.Contains("shared_"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_B;
        }

        if (glyph.EndsWith("button_x.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_X;
        }

        if (glyph.EndsWith("button_y.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_Y;
        }

        if (glyph.EndsWith("_l1.svg") || glyph.EndsWith("_l2.svg") || glyph.EndsWith("_l.svg") ||
            glyph.EndsWith("_sl.svg") || glyph.EndsWith("_lb.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_L;
        }

        if (glyph.EndsWith("_r1.svg") || glyph.EndsWith("_r2.svg") || glyph.EndsWith("_r.svg") ||
            glyph.EndsWith("_sr.svg") || glyph.EndsWith("_rb.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_R;
        }

        if (glyph.EndsWith("_dpad.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_SELECT;
        }

        if (glyph.EndsWith("button_triangle.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP;
        }

        if (glyph.EndsWith("button_square.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_LEFT;
        }

        if (glyph.EndsWith("button_x.svg") && glyph.Contains("ps_"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN;
        }

        if (glyph.EndsWith("button_circle.svg"))
        {
            return RetroBindings.RETRO_DEVICE_ID_JOYPAD_RIGHT;
        }

        return ButtonNamesMap[button];
    }
}