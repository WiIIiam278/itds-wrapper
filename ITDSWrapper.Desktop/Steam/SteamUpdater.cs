using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamUpdater(SdlInputDriver inputDriver, SteamHelperIpc ipc) : IUpdater
{
    public int Update()
    {
        if (!inputDriver.HasInputContext)
        {
            inputDriver.SetInputContext();
            return inputDriver.HasInputContext ? 1 : -1;
        }
        
        inputDriver.PumpView();
        
        return -1;
    }

    public void Die()
    {
        ipc.SendCommand("DIE");
    }
}