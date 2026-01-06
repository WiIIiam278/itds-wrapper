using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ITDSWrapper.ViewModels;
using ITDSWrapper.Views.Controls;

namespace ITDSWrapper.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        OnScreenControls.AddHandler(PointerPressedEvent, OnScreenControls_OnPointerPressed, handledEventsToo: true);
        OnScreenControls.AddHandler(PointerReleasedEvent, OnScreenControls_OnPointerReleased, handledEventsToo: true);
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

    private void OnScreenControls_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        HandleOnScreenControls(sender, e, false);
    }

    private void OnScreenControls_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        HandleOnScreenControls(sender, e, false);
    }

    private void OnScreenControls_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        HandleOnScreenControls(sender, e, true);
    }

    private void HandleOnScreenControls(object? sender, PointerEventArgs e, bool release)
    {
        if (sender is null)
        {
            return;
        }

        ((MainViewModel)DataContext!).CurrentInputDriver = 0;
        
        Grid grid = (sender as Grid)!;
        foreach (Control control in grid.Children)
        {
            if (control is Grid subGrid)
            {
                Point pos = e.GetPosition(subGrid);
                foreach (var button in subGrid.Children.Cast<IPressableButtonView>())
                {
                    CheckButtonPressed(pos, button, release);
                }
            }
            else if (control is IPressableButtonView button)
            {
                CheckButtonPressed(e.GetPosition(grid), button, release);
            }
        }
    }
    
    

    private void CheckButtonPressed(Point pos, IPressableButtonView view, bool release)
    {
        if (pos.X >= view.Bounds.Left && pos.Y >= view.Bounds.Top && pos.X <= view.Bounds.Right &&
            pos.Y <= view.Bounds.Bottom)
        {
            if (release)
            {
                view.ReleaseButton();
            }
            else
            {
                view.PressButton();
            }
        }
        else if (!release)
        {
            view.ReleaseButton(softRelease: true);
        }
    }

    private void MainScreen_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Point pos = e.GetPosition(sender as Control);
        // If we touch anywhere except the bottom screen, reveal the virtual controls
        if (MainViewModel.IsMobile && (pos.X < DsScreen.Bounds.Left || pos.Y < DsScreen.Bounds.Top + DsScreen.Bounds.Height / 2 ||
                                       pos.X > DsScreen.Bounds.Right || pos.Y > DsScreen.Bounds.Bottom))
        {
            ((MainViewModel)DataContext!).CurrentInputDriver = 0;
        }
    }
}