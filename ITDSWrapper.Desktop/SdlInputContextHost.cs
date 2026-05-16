using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Silk.NET.Core.Loader;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;
using Window = Silk.NET.Windowing.Window;

namespace ITDSWrapper.Desktop;

public sealed class SdlInputContextHost : IDisposable
{
    private TopLevel? _topLevel;

    public IView? View { get; private set; }

    public SdlInputContextHost()
    {
        SdlWindowing.RegisterPlatform();
    }

    public void Attach(TopLevel topLevel)
    {
        if (_topLevel == topLevel && View is { IsInitialized: true })
        {
            return;
        }

        Detach();

        if (OperatingSystem.IsLinux())
        {
            ((DefaultPathResolver)PathResolver.Default).Resolvers.Clear();
            ((DefaultPathResolver)PathResolver.Default).Resolvers.Add(file => 
                AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") is string nativeDllSearchDirectories
                    ? nativeDllSearchDirectories.Split(":").Select(dir => Path.Combine(dir, file))
                    : []);
        }

        _topLevel = topLevel;
        _topLevel.Closed += HandleTopLevelClosed;

        Dispatcher.UIThread.Invoke(() =>
        {
            View = Window.Create(new()
            {
                IsVisible = false,
                API = GraphicsAPI.None,
                WindowBorder = WindowBorder.Hidden,
                Size = new(1, 1),
                Title = "InputCapture",
            });
            
            SdlProvider.SDL.Value.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");
            View!.Initialize();

            if (View is IWindow window)
                window.IsVisible = false;
        });
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
