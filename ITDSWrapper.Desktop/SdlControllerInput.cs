using System;
using ITDSWrapper.Input;
using Silk.NET.Input;

namespace ITDSWrapper.Desktop;

public enum ThumbstickDirection
{
    POSITIVE_X,
    POSITIVE_Y,
    NEGATIVE_X,
    NEGATIVE_Y,
}

public class SdlControllerInput : IGameInput<object>
{
    private object? _input;
    private ThumbstickDirection? _direction;

    public bool IsSet { get; set; }
    public Action? SpecialAction { get; set; }

    public SdlControllerInput(object? input)
    {
        SetInput(input);
    }

    public void SetInput(object? input)
    {
        if (input is Button button)
        {
            _input = button;
            _direction = null;
        }
        else if (input is (Thumbstick thumbstick, ThumbstickDirection direction))
        {
            _input = thumbstick;
            _direction = direction;
        }
        else if (input is Trigger trigger)
        {
            _input = trigger;
        }
    }

    public void Press(object? input)
    {
        if (_input == input && _input is Button { Pressed: true })
        {
            IsSet = true;
            SpecialAction?.Invoke();
        }
        else if (_input == input && _input is Thumbstick thumbstick)
        {
            if (_direction == ThumbstickDirection.NEGATIVE_X && thumbstick.X <= -0.1f ||
                _direction == ThumbstickDirection.POSITIVE_X && thumbstick.X >= 0.1f ||
                _direction == ThumbstickDirection.POSITIVE_Y && thumbstick.Y >= 0.1f ||
                _direction == ThumbstickDirection.NEGATIVE_Y && thumbstick.Y < -0.1f)
            {
                IsSet = true;
                SpecialAction?.Invoke();
            }
        }
        else if (_input == input && _input is Trigger { Position: >= 0.1f })
        {
            IsSet = true;
            SpecialAction?.Invoke();
        }
    }

    public void Release(object? input)
    {
        if (_input == input && _input is Button { Pressed: false })
        {
            IsSet = false;
        }
        else if (_input == input && _input is Thumbstick thumbstick)
        {
            if (_direction == ThumbstickDirection.NEGATIVE_X && thumbstick.X > -0.1f ||
                _direction == ThumbstickDirection.POSITIVE_X && thumbstick.X < 0.1f ||
                _direction == ThumbstickDirection.POSITIVE_Y && thumbstick.Y < 0.1f ||
                _direction == ThumbstickDirection.NEGATIVE_Y && thumbstick.Y > -0.1f)
            {
                IsSet = false;
            }
        }
        else if (_input == input && _input is Trigger { Position: < 0.1f })
        {
            IsSet = false;
        }
    }
}