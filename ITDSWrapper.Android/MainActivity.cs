using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using ITDSWrapper.Core;

namespace ITDSWrapper.Android;

[Activity(
    Label = "Into the Dream Spring",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private EmulationDriver? _driver;
    
    public override void OnWindowFocusChanged(bool hasFocus)
    {
        _driver?.PushPauseState(!hasFocus);
        base.OnWindowFocusChanged(hasFocus);
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        _driver = new();
        
        return base
            .CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            .AfterSetup(b =>
            {
                ((App)b.Instance!).AudioBackend = new AndroidAudioBackend();
                ((App)b.Instance!).Driver = _driver;
            });
    }
}