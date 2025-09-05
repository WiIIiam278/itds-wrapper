using Libretro.NET;

namespace ITDSWrapper.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    public MainViewModel()
    {
        RetroWrapper retro = new();
        retro.LoadCore("");
    }
}