using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamAchievementManager(SteamHelperIpc ipc) : IAchievementManager
{
    public void Unlock(string achievementName)
    {
        ipc.SendCommand($"ACHIEVEMENT {achievementName}");
    }
}