using Libretro.NET;

namespace ITDSWrapper.Input;

public class InputSwitcher
{
    private RetroWrapper.OnCheckInputDelegate? _defaultInputDelegate;
    
    public RetroWrapper? Wrapper { private get; set; }

    public void SetDefaultInputDelegate(RetroWrapper.OnCheckInputDelegate defaultDelegate)
    {
        _defaultInputDelegate = defaultDelegate;
    }
    
    public void SetInputDelegate(RetroWrapper.OnCheckInputDelegate inputDelegate)
    {
        Wrapper?.OnCheckInput = inputDelegate;
    }

    public void ResetInputDelegate()
    {
        Wrapper?.OnCheckInput = _defaultInputDelegate;
    }
}