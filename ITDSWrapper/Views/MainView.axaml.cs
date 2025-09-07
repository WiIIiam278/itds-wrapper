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

    private void DsScreen_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ((MainViewModel)DataContext!).HandlePointer(DsScreen, pressedArgs: e);
    }

    private void DsScreen_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ((MainViewModel)DataContext!).HandlePointer(DsScreen, releasedArgs: e);
    }

    private void DsScreen_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        ((MainViewModel)DataContext!).HandlePointer(DsScreen, movedArgs: e);
    }
}