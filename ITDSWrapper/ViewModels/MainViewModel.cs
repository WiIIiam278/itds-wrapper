using Libretro.NET;

namespace ITDSWrapper.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    public MainViewModel()
    {
        RetroWrapper wrapper = new();
        wrapper.LoadCore();
        wrapper.LoadGame("into-the-dream-spring.nds");
    }
}