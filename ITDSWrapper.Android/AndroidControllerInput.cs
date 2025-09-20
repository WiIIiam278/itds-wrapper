using System;
using Android.Views;
using ITDSWrapper.Input;

namespace ITDSWrapper.Android;

public class AndroidControllerInput : IGameInput<AndroidInputContainer?>
{
    private AndroidInputContainer? _input;
    
    public bool IsSet { get; set; }
    public Action? SpecialAction { get; set; }
    
    public AndroidControllerInput(AndroidInputContainer input, Action? specialAction = null)
    {
        _input = input;
        SpecialAction = specialAction;
    }
    
    public void SetInput(AndroidInputContainer? input)
    {
        _input = input;
    }

    public void Press(AndroidInputContainer? input)
    {
        if (input?.Type == AndroidInputType.KEY && input.Key == _input?.Key)
        {
            IsSet = true;
            SpecialAction?.Invoke();
        }
    }

    public void Release(AndroidInputContainer? input)
    {
        if (input?.Type == AndroidInputType.KEY && input.Key == _input?.Key)
        {
            IsSet = false;
        }
    }
}

public enum AndroidInputType
{
    KEY,
    MOTION,
}

public record AndroidInputContainer(AndroidInputType Type, Keycode? Key, MotionRange? Motion);