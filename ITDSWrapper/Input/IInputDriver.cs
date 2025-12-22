namespace ITDSWrapper.Input;

public interface IInputDriver
{
    public bool RequestInputUpdate { get; set; }

    public void Shutdown();
    public uint[] GetInputKeys();
    public void SetActionSet(string actionSet);
    public void SetBinding<T>(uint input, IGameInput<T>? binding);
    public bool QueryInput(uint id);
    public void Push<T>(T binding);
    public void Release<T>(T binding);
    public void DoRumble(ushort strength);
}