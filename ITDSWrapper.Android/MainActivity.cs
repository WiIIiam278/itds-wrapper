using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Util;
using Android.Views;
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
    private AndroidControllerInputDriver? _controllerInputDriver;
    private AndroidUpdater? _updater;
    
    public override void OnWindowFocusChanged(bool hasFocus)
    {
        _pauseDriver?.PushPauseState(!hasFocus);
        base.OnWindowFocusChanged(hasFocus);
    }

    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        if (e?.DeviceId != _controllerInputDriver?.Controller?.Id)
        {
            return base.OnKeyDown(keyCode, e);
        }
        _controllerInputDriver?.Push(new AndroidInputContainer(AndroidInputType.KEY, keyCode, null));
        if (_updater is not null)
        {
            _updater.RetValue = 0;
        }

        return true;
    }

    public override bool OnKeyUp(Keycode keyCode, KeyEvent? e)
    {
        if (e?.DeviceId != _controllerInputDriver?.Controller?.Id)
        {
            return base.OnKeyDown(keyCode, e);
        }
        _controllerInputDriver?.Release(new AndroidInputContainer(AndroidInputType.KEY, keyCode, null));
        
        return true;
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
        _controllerInputDriver = new();
        _updater = new(_controllerInputDriver);
        
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
                ((App)b.Instance).InputDrivers = [_controllerInputDriver];
                ((App)b.Instance).Updater = _updater;
            });
    }
}