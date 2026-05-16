using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamUpdater(SdlInputDriver inputDriver, SteamHelperIpc ipc) : IUpdater
{
    public int Update()
    {
        
        return -1;
    }

    public void Die()
    {
        ipc.SendCommand("DIE");
    }
}