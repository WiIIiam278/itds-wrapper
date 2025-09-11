using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
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
    private PauseDriver? _pauseDriver;
    private AndroidHapticsBackend? _hapticsBackend;
    
    public override void OnWindowFocusChanged(bool hasFocus)
    {
        _pauseDriver?.PushPauseState(!hasFocus);
        base.OnWindowFocusChanged(hasFocus);
    }

    public override View? OnCreateView(View? parent, string name, Context context, IAttributeSet attrs)
    {
        View? view = base.OnCreateView(parent, name, context, attrs);
        if (_hapticsBackend is not null && view is not null)
        {
            _hapticsBackend.View = view;
        }
        else if (_hapticsBackend is not null && parent is not null)
        {
            _hapticsBackend.View = parent;
        }
        return view;
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        _pauseDriver = new();
        _hapticsBackend = new();
        
        return base
            .CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            .AfterSetup(b =>
            {
                AndroidAudioBackend audioBackend = new();
                ((App)b.Instance!).AudioBackend = audioBackend;
                ((App)b.Instance).PauseDriver = _pauseDriver;
                ((App)b.Instance).HapticsBackend = _hapticsBackend;
            });
    }
}