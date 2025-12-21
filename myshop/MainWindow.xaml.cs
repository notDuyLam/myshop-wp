using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using myshop.Helpers;
using myshop.Services;
using myshop.Views;

namespace myshop
{
    /// <summary>
    /// Main window với NavigationView shell
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly NavigationService _navigationService;

        public MainWindow()
        {
            InitializeComponent();
            
            // Register this window with WindowHelper
            WindowHelper.RegisterWindow(this);
            
            // Get NavigationService from DI
            _navigationService = App.ServiceProvider?.GetService<NavigationService>() 
                ?? throw new InvalidOperationException("NavigationService not found in DI container");
            
            // Initialize NavigationService với ContentFrame
            _navigationService.Initialize(ContentFrame);
            
            // Navigate to default page (Dashboard)
            _navigationService.NavigateTo(typeof(DashboardPage));
            
            // Set default selected item
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
            {
                var pageType = tag switch
                {
                    "Dashboard" => typeof(DashboardPage),
                    "Products" => typeof(ProductsPage),
                    "Orders" => typeof(OrdersPage),
                    "Report" => typeof(ReportPage),
                    "Settings" => typeof(SettingsPage),
                    _ => typeof(DashboardPage)
                };
                
                _navigationService.NavigateTo(pageType);
            }
        }
    }
}
