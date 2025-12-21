using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using myshop.ViewModels;

namespace myshop.Views;

public sealed partial class ConfigPage : Page
{
    public ConfigViewModel ViewModel { get; }

    public ConfigPage()
    {
        InitializeComponent();
        
        // Set DataContext from DI
        ViewModel = App.ServiceProvider?.GetService<ConfigViewModel>()
            ?? throw new InvalidOperationException("ConfigViewModel not found in DI container");
        
        DataContext = ViewModel;
        
        // Sync controls với ViewModel
        PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        PortTextBox.TextChanged += PortTextBox_TextChanged;
        
        // Sync initial values từ ViewModel
        Loaded += ConfigPage_Loaded;
    }

    private void ConfigPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Sync PortTextBox với ViewModel khi page loaded
        PortTextBox.Text = ViewModel.Port.ToString();
    }

    private void PasswordBox_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            ViewModel.Password = passwordBox.Password;
        }
    }

    private void PortTextBox_TextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && int.TryParse(textBox.Text, out var port))
        {
            ViewModel.Port = port;
        }
    }
}

