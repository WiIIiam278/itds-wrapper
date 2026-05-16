using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamUpdater(SdlInputDriver inputDriver, SteamHelperIpc ipc) : IUpdater
{
    public int Update()
    {
        if (!inputDriver.HasInputContext)
        {
            inputDriver.SetInputContext();
            return inputDriver.HasGamepad ? 1 : -1;
        }
        
        return inputDriver.PumpView();
    }

    public void Die()
    {
        ipc.SendCommand("DIE");
    }
}