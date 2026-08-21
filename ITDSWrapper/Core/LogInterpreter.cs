using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using ITDSWrapper.Accessibility;

namespace ITDSWrapper.Core;

public partial class LogInterpreter : IDisposable
{
    protected const string WrapperLogPrefix = "[WRAPPER] ";

    private const string AchievementUnlockedVerb = "ACHIEVEMENT_UNLOCKED";
    private const string BorderSetVerb = "BORDER_SET";
    private const string ScreenReaderVerb = "SCREEN_READER";
    private const string StartupReceivedVerb = "STARTUP_RECEIVED";
    private const string LanguageReceivedVerb = "LANG_RECEIVED";
    private const string WarningVerb = "WARNING";
    private const string SaveTraceVerb = "SAVE_TRACE";

    public bool WatchForSdCreate { get; set; }
    public Action<string>? SetNextBorder { get; set; }

    public IAchievementManager? AchievementManager { get; set; }
    public IScreenReader? ScreenReader { get; set; }

    public bool StartupReceived { get; private set; }
    public bool LangReceived { get; private set; }

    private readonly string? _discordWebhookUri;
    private readonly DropOutQueue<string> _recentLogs = new(500);
    private readonly List<string> _saveTrace = [];

    public LogInterpreter()
    {
        foreach (var attr in Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (attr.Key == "DiscordWebhookUri")
            {
                _discordWebhookUri = attr.Value;
            }
        }
    }

    public virtual int InterpretLog(string log)
    {
        _recentLogs.Add(log);

        if (log.Contains("ARM9: data abort") || log.Contains("ARM9: prefetch abort"))
        {
            SendWebhookLog("Abort", log).GetAwaiter().GetResult();
            return -1;
        }

        // The SD card is initialized after the game boots -- if we replace the SD card image here, it will load properly
        int wrapperPrefixLocation = WatchForSdCreate && log.Contains("[melonDS] Game is now booting")
            ? 0
            : log.IndexOf(WrapperLogPrefix, StringComparison.Ordinal);
        if (wrapperPrefixLocation < 0)
        {
            return wrapperPrefixLocation;
        }

        int startIndex = wrapperPrefixLocation + WrapperLogPrefix.Length;
        int endIndex = log.IndexOf(':', startIndex);
        string verb = log[startIndex..endIndex];

        string logParam = log[(endIndex + 2)..^1];
        switch (verb)
        {
            case AchievementUnlockedVerb:
                AchievementManager?.Unlock(logParam);
                break;

            case BorderSetVerb:
                SetNextBorder?.Invoke(logParam);
                break;

            case ScreenReaderVerb:
                ScreenReader?.Speak(TextColorRegex().Replace(logParam, ""));
                break;

            case StartupReceivedVerb:
                StartupReceived = true;
                break;

            case LanguageReceivedVerb:
                LangReceived = true;
                break;

            case WarningVerb:
                SendWebhookLog("Warning", logParam).GetAwaiter().GetResult();
                break;

            case SaveTraceVerb:
                if (logParam.StartsWith("SCENE"))
                {
                    SendWebhookLog("Crash", $"In scene {logParam}").GetAwaiter().GetResult();
                }

                _saveTrace.Add(logParam);
                if (logParam.StartsWith("DONE"))
                {
                    SendWebhookFile("Save Trace", $"savetrace.txt", _saveTrace).GetAwaiter().GetResult();
                }
                break;
        }

        return wrapperPrefixLocation;
    }

    private async Task SendWebhookLog(string title, string description)
    {
        using DiscordWebhookClient client = new(_discordWebhookUri);

        EmbedBuilder embed = new();
        embed.WithTitle(title).WithDescription(description);
        await client.SendMessageAsync(embeds: [embed.Build()]);

        await SendWebhookFile("Log", $"{title}.log", _recentLogs.GetList());
    }

    private async Task SendWebhookFile(string text, string filename, List<string> lines)
    {
        using DiscordWebhookClient client = new(_discordWebhookUri);

        MemoryStream fileStream = new();
        StreamWriter writer = new(fileStream);
        await writer.WriteAsync(string.Join('\n', lines.Select(l => l.Trim())));
        await writer.FlushAsync();
        await client.SendFileAsync(text: text, filename: filename, stream: fileStream);
    }

    public void Dispose()
    {
        ScreenReader?.Dispose();
    }

    [GeneratedRegex(@"\@\d")]
    private static partial Regex TextColorRegex();
}