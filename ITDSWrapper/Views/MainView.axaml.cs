using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ITDSWrapper.ViewModels;

namespace ITDSWrapper.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
    }

    private void ScreenGrid_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ((MainViewModel)DataContext!).EmuRenderWidth = Math.Min(e.NewSize.Width, e.NewSize.Height * (256.0 / 384.0));
        ((MainViewModel)DataContext).EmuRenderHeight = Math.Min(e.NewSize.Height, e.NewSize.Width * (384.0 / 256.0));
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