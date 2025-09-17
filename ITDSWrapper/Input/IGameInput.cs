using System;

namespace ITDSWrapper.Input;

public interface IGameInput<in T> : IGameInputSettable
{
    public Action? SpecialAction { get; set; }
    public void SetInput(T? input);
    public void Press(T? input);
    public void Release(T? input);
}

public interface IGameInputSettable
{
    public bool IsSet { get; protected set; }
}