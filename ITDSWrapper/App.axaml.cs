using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ITDSWrapper.Audio;
using ITDSWrapper.Core;
using ITDSWrapper.Haptics;
using ITDSWrapper.Input;
using ITDSWrapper.ViewModels;
using ITDSWrapper.Views;

namespace ITDSWrapper;

public partial class App : Application
{
    public IAudioBackend? AudioBackend { get; set; }
    public IInputDriver? InputDriver { get; set; }
    public IHapticsBackend? HapticsBackend { get; set; }
    public IUpdater? Updater { get; set; }
    public LogInterpreter? LogInterpreter { get; set; }
    public PauseDriver? PauseDriver { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}