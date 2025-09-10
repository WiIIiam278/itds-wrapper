using ITDSWrapper.ViewModels.Controls;

namespace ITDSWrapper.Input;

public class VirtualButtonInput : IGameInput<VirtualButtonViewModel>
{
    public bool IsSet { get; set; }
    
    public void SetInput(VirtualButtonViewModel? input)
    {
        if (input is not null)
        {
            input.AssociatedInput = this;
        }
    }

    public void Press(VirtualButtonViewModel? input)
    {
        IsSet = input is not null || IsSet;
    }

    public void Release(VirtualButtonViewModel? input)
    {
        IsSet = input is null && IsSet;
    }
}