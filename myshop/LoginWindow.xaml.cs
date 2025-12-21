using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using myshop.Helpers;
using myshop.Services;
using myshop.Views;
using Windows.Graphics;
using WinRT.Interop;

namespace myshop;

/// <summary>
/// Login Window - Entry point của ứng dụng
/// </summary>
public sealed partial class LoginWindow : Microsoft.UI.Xaml.Window
{
    private readonly NavigationService _navigationService;

    public LoginWindow()
    {
        InitializeComponent();

        // Register this window with WindowHelper
        WindowHelper.RegisterWindow(this);

        // Set window size (WinUI 3 requires setting size in code-behind)
        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        
        if (appWindow != null)
        {
            appWindow.Resize(new SizeInt32(900, 600));
        }

        // Get NavigationService from DI
        _navigationService = App.ServiceProvider?.GetService<NavigationService>()
            ?? throw new InvalidOperationException("NavigationService not found in DI container");

        // Initialize NavigationService với ContentFrame
        _navigationService.Initialize(ContentFrame);

        // Navigate to LoginPage
        _navigationService.NavigateTo(typeof(LoginPage));
    }
}

