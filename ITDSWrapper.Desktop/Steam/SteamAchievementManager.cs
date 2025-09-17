using System.Linq;
using ITDSWrapper.Core;
using Steamworks;

namespace ITDSWrapper.Desktop.Steam;

public class SteamAchievementManager : IAchievementManager
{
    public void Unlock(string achievementName)
    {
        SteamUserStats.SetAchievement(achievementName);
    }
}