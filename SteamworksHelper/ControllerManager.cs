using System.Collections.Generic;
using Steamworks;

namespace SteamworksHelper;

public class ControllerPollResponse
{
    public byte HasController;
    public byte NewController;
}

public class ControllerAnalogResponse
{
    public byte Up;
    public byte Right;
    public byte Down;
    public byte Left;
}

public class ControllerManager
{
    private readonly List<Controller> _controllers = [];
    private Controller _currentController;
    
    public ControllerPollResponse PollControllers()
    {
        ControllerPollResponse pollResponse = new();
        
        SteamInput.GetControllerNoAlloc(_controllers);
        if (_controllers.Count > 0)
        {
            pollResponse.HasController = 1;
            if (_currentController != _controllers[0])
            {
                pollResponse.NewController = 1;
            }
            
            _currentController = _controllers[0];
        }
        return pollResponse;
    }

    public void SetActionSet(string set)
    {
        _currentController.ActionSet = set;
    }

    public ControllerAnalogResponse GetAnalogState(string action)
    {
        ControllerAnalogResponse analogResponse = new();
        AnalogState state = _currentController.GetAnalogState(action);
        if (state.X > 0.05f)
        {
            analogResponse.Right = 1;
        }
        else if (state.X < -0.05f)
        {
            analogResponse.Left = 1;
        }

        if (state.Y < -0.05f)
        {
            analogResponse.Down = 1;
        }
        else if (state.Y > 0.05f)
        {
            analogResponse.Up = 1;
        }

        return analogResponse;
    }

    public bool GetDigitalState(string action)
    {
        return _currentController.GetDigitalState(action).Pressed;
    }

    public string? GetGlyph(string actionName)
    {
        return SteamInput.GetSvgActionGlyph(_currentController, actionName);
    }

    public void Rumble(ushort strength)
    {
        _currentController.TriggerVibration(strength, strength);
    }
}