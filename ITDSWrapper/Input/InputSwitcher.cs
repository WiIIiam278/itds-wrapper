using System.Collections.Generic;
using Libretro.NET;

namespace ITDSWrapper.Input;

public class InputSwitcher
{
    private RetroWrapper.OnCheckInputDelegate? _defaultInputDelegate;
    private readonly List<InputDelegate> _queue = [];
    private string _currentDelegateId = "default";
    
    public RetroWrapper? Wrapper { private get; set; }

    public void SetDefaultInputDelegate(RetroWrapper.OnCheckInputDelegate defaultDelegate)
    {
        _defaultInputDelegate = defaultDelegate;
    }
    
    public void SetInputDelegate(InputDelegate inputDelegate)
    {
        if (inputDelegate.Id == _currentDelegateId)
        {
            return;
        }
        if (Wrapper?.OnCheckInput is null || _currentDelegateId == "default")
        {
            _currentDelegateId = inputDelegate.Id;
            Wrapper?.OnCheckInput = inputDelegate.InputFunction;
        }
        else
        {
            _queue.Add(inputDelegate);
        }
    }

    public void ResetInputDelegate()
    {
        if (_queue.Count > 0)
        {
            _currentDelegateId = _queue[0].Id;
            Wrapper?.OnCheckInput = _queue[0].InputFunction;
            _queue.RemoveAt(0);
        }
        else
        {
            _currentDelegateId = "default";
            Wrapper?.OnCheckInput = _defaultInputDelegate;
        }
    }
}

public record InputDelegate(string Id, RetroWrapper.OnCheckInputDelegate? InputFunction);