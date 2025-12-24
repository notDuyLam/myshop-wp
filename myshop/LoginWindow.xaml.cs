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

        // Set window size và icon
        SetupWindow();

        // Get NavigationService from DI
        _navigationService = App.ServiceProvider?.GetService<NavigationService>()
            ?? throw new InvalidOperationException("NavigationService not found in DI container");

        // Initialize NavigationService với ContentFrame
        _navigationService.Initialize(ContentFrame);

        // Navigate to LoginPage
        _navigationService.NavigateTo(typeof(LoginPage));
    }

    /// <summary>
    /// Thiết lập window size và icon
    /// </summary>
    private void SetupWindow()
    {
        try
        {
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                // Set window size
                appWindow.Resize(new SizeInt32(900, 600));

                // ✅ Set icon cho title bar
                appWindow.SetIcon("Assets/app.ico");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to setup window: {ex.Message}");
        }
    }
}

