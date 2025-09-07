using Avalonia.Controls;
using Avalonia.Input;
using ITDSWrapper.ViewModels;

namespace ITDSWrapper.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void Screen_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ((MainViewModel)DataContext!).HandlePointer(Screen, pressedArgs: e);
    }

    private void Screen_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ((MainViewModel)DataContext!).HandlePointer(Screen, releasedArgs: e);
    }

    private void Screen_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        ((MainViewModel)DataContext!).HandlePointer(Screen, movedArgs: e);
    }
}