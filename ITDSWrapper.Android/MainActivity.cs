using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

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
        return base
            .CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            .AfterSetup(b => ((App)b.Instance!).AudioBackend = new AndroidAudioBackend());
    }
}