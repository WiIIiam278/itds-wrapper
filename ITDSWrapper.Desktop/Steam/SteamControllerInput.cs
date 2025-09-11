using ITDSWrapper.Input;

namespace ITDSWrapper.Desktop.Steam;

public class SteamControllerInput(string actionName) : IGameInput<string>
{
    private string _actionName = actionName;
    public bool IsSet { get; set; }

    public void SetInput(string? input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            _actionName = input;
        }
    }

    public void Press(string? input)
    {
        if (!string.IsNullOrEmpty(input) && _actionName == input)
        {
            IsSet = true;
        }
    }

    public void Release(string? input)
    {
        if (!string.IsNullOrEmpty(input) && _actionName == input)
        {
            IsSet = false;
        }
    }
}

public record SteamInputAction(string ActionName, uint[] RetroBindings);