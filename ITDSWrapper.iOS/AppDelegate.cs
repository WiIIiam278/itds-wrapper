using System.Globalization;
using Foundation;
using Avalonia;
using Avalonia.iOS;
using Avalonia.ReactiveUI;
using AvFoundationBackend;
using ITDSWrapper.Core;

namespace ITDSWrapper.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            .AfterSetup(b =>
            {
                IosControllerInputDriver inputDriver = new();
                ((App)b.Instance!).AudioBackend = new AvFoundationAudioBackend();
                ((App)b.Instance).PauseDriver = new(useActivatableLifetime: true);
                ((App)b.Instance).HapticsBackend = new IosHapticsBackend();
                ((App)b.Instance).InputDrivers = [inputDriver];
                ((App)b.Instance).Updater = new IosUpdater(inputDriver);
                ((App)b.Instance).ScreenReader = new AvFoundationScreenReader(
                    CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
                    {
                        _ => "en-GB",
                    });
            });
    }
}