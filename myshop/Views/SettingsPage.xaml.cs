using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using myshop.Helpers;
using myshop.Services;
using Windows.Storage;

namespace myshop.Views;

public sealed partial class SettingsPage : Page
{
    private readonly NavigationService _navigationService;
    private readonly ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

    public SettingsPage()
    {
        InitializeComponent();
        
        // Get NavigationService from DI
        _navigationService = App.ServiceProvider?.GetService<NavigationService>()
            ?? throw new InvalidOperationException("NavigationService not found in DI container");
    }

    private void ConfigServerButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _navigationService.NavigateTo(typeof(ConfigPage));
    }

    private async void LogoutButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Xóa trạng thái đăng nhập
        settings.Values["is_logged_in"] = false;
        
        // Lấy MainWindow hiện tại
        var mainWindow = WindowHelper.GetWindow<MainWindow>();
        
        if (mainWindow != null)
        {
            // Tạo LoginWindow mới trước khi đóng MainWindow
            var loginWindow = new LoginWindow();
            loginWindow.Activate();
            
            // Delay một chút để đảm bảo LoginWindow đã sẵn sàng
            await Task.Delay(200);
            
            // Đóng MainWindow
            mainWindow.Close();
        }
    }
}

