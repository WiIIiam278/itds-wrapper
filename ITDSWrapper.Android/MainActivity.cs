using Android.App;
using Android.Content;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using NAudio.Sdl2;
using NAudio.Sdl2.Structures;
using Org.Libsdl.App;

namespace ITDSWrapper.Android;

[Activity(
    Label = "Into the Dream Spring",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        SDL.LoadLibrary("SDL2");
        SDL.SetupJNI();
        SDL.Initialize();
        SDL.Context = this;
        
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI();
    }
}