using System;

namespace ITDSWrapper.Core;

public class LogInterpreter
{
    protected const string WrapperLogPrefix = "[WRAPPER] ";
    
    public virtual int InterpretLog(string log)
    {
        return log.IndexOf(WrapperLogPrefix, StringComparison.Ordinal);
    }
}