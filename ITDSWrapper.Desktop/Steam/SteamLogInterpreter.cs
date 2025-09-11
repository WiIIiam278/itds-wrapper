using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamLogInterpreter(SteamInputDriver inputDriver) : LogInterpreter
{
    private const string ActionSetVerb = "ACTION_SET";
    
    public override int InterpretLog(string log)
    {
        int wrapperPrefixLocation = base.InterpretLog(log);
        if (wrapperPrefixLocation < 0)
        {
            return wrapperPrefixLocation;
        }

        int startIndex = wrapperPrefixLocation + WrapperLogPrefix.Length;
        int endIndex = log.IndexOf(':', startIndex);
        string verb = log[startIndex..endIndex];

        switch (verb)
        {
            case ActionSetVerb:
                inputDriver.SetActionSet(log[(endIndex + 2)..^1]);
                break;
        }

        return wrapperPrefixLocation;
    }
}