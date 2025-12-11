using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace myshop.Views;

public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        InitializeComponent();
        
        // Set version info
        var version = Package.Current.Id.Version;
        VersionTextBlock.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
    }

    private void LoginButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // TODO: Implement login logic in ViewModel
    }

    private void ConfigButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // TODO: Navigate to ConfigPage
    }
}

