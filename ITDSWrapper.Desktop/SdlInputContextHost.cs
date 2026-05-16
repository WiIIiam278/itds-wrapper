using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform;
using Silk.NET.Core.Loader;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;

namespace ITDSWrapper.Desktop;

public sealed class SdlInputContextHost : IDisposable
{
    private TopLevel? _topLevel;

    public IView? View { get; private set; }

    public SdlInputContextHost()
    {
        SdlWindowing.RegisterPlatform();
    }

    public unsafe void Attach(TopLevel topLevel)
    {
        if (_topLevel == topLevel && View is { IsInitialized: true })
        {
            return;
        }

        Detach();

        IPlatformHandle? platformHandle = topLevel.TryGetPlatformHandle();
        if (platformHandle is null)
        {
            throw new InvalidOperationException("Unable to create the SDL input host before Avalonia exposes a native platform handle.");
        }

        _topLevel = topLevel;
        _topLevel.Closed += HandleTopLevelClosed;

        if (OperatingSystem.IsLinux())
        {
            ((DefaultPathResolver)PathResolver.Default).Resolvers.Clear();
            ((DefaultPathResolver)PathResolver.Default).Resolvers.Add(file => 
                AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") is string nativeDllSearchDirectories
                    ? nativeDllSearchDirectories.Split(":").Select(dir => Path.Combine(dir, file))
                    : []);
        }
        View = SdlWindowing.CreateFrom(platformHandle.Handle.ToPointer());
        if (!View.IsInitialized)
        {
            View.Initialize();
        }
    }

    public void Dispose()
    {
        Detach();
    }

    private void HandleTopLevelClosed(object? sender, EventArgs e)
    {
        Detach();
    }

    private void Detach()
    {
        if (_topLevel is not null)
        {
            _topLevel.Closed -= HandleTopLevelClosed;
            _topLevel = null;
        }

        View?.Dispose();
        View = null;
    }
}
