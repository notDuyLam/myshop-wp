using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using myshop.Helpers;
using myshop.Services;
using myshop.Views;
using System;
using Windows.Graphics;
using WinRT.Interop;

namespace myshop
{
    /// <summary>
    /// Main window với NavigationView shell - Responsive
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly NavigationService _navigationService;
        private Type _currentPageType;
        private const int CompactModeThreshold = 600; // Breakpoint for compact mode
        private bool _navPaneOpenBeforeCompact = true; // nhớ trạng thái trước khi vào compact
        private bool _isInCompactMode = false;

        public MainWindow()
        {
            InitializeComponent();

            SetupWindow();

            // Register this window with WindowHelper
            WindowHelper.RegisterWindow(this);

            // Get NavigationService from DI
            _navigationService = App.ServiceProvider?.GetService<NavigationService>()
                ?? throw new InvalidOperationException("NavigationService not found in DI container");

            // Initialize NavigationService với ContentFrame
            _navigationService.Initialize(ContentFrame);

            // Subscribe to frame navigation events
            ContentFrame.Navigated += OnFrameNavigated;

            // Subscribe to window size changed
            this.SizeChanged += MainWindow_SizeChanged;


            // Set initial display mode based on window size
            UpdateNavigationViewDisplayMode();

            // Navigate to default page (Dashboard)
            _navigationService.NavigateTo(typeof(DashboardPage));
            _currentPageType = typeof(DashboardPage);

            // Set default selected item
            NavView.SelectedItem = NavView.MenuItems[0];
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

                // Sửa: Gọi trực tiếp từ class AppWindow, không qua biến appWindow
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow != null)
                {
                    appWindow.SetIcon("Assets/app.ico");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to setup window: {ex.Message}");
            }
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            UpdateNavigationViewDisplayMode();
        }

        private void UpdateNavigationViewDisplayMode()
        {
            // Get current window width
            double width = this.Bounds.Width;
            bool shouldBeCompact = width < CompactModeThreshold;

            if (shouldBeCompact && !_isInCompactMode)
            {
                // Chuyển từ rộng sang nhỏ: nhớ trạng thái và đóng navi
                _navPaneOpenBeforeCompact = NavView.IsPaneOpen;
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
                NavView.IsPaneOpen = false;
                _isInCompactMode = true;
            }
            else if (!shouldBeCompact && _isInCompactMode)
            {
                // Chuyển từ nhỏ sang rộng: khôi phục trạng thái
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                NavView.IsPaneOpen = _navPaneOpenBeforeCompact;
                _isInCompactMode = false;
            }
            // Nếu không đổi mode, không can thiệp trạng thái mở/đóng
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

                // Prevent re-navigation to the same page
                if (pageType != _currentPageType)
                {
                    _navigationService.NavigateTo(pageType);
                    _currentPageType = pageType;
                }

                // Auto-close pane in compact mode after selection
                if (NavView.PaneDisplayMode == NavigationViewPaneDisplayMode.LeftCompact)
                {
                    NavView.IsPaneOpen = false;
                }
            }
        }

        private void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            // Sync NavigationView selection with frame navigation
            _currentPageType = e.SourcePageType;

            // Update selected item based on current page
            foreach (var menuItem in NavView.MenuItems)
            {
                if (menuItem is NavigationViewItem item && item.Tag is string tag)
                {
                    var pageType = tag switch
                    {
                        "Dashboard" => typeof(DashboardPage),
                        "Products" => typeof(ProductsPage),
                        "Orders" => typeof(OrdersPage),
                        "Report" => typeof(ReportPage),
                        "Settings" => typeof(SettingsPage),
                        _ => null
                    };

                    if (pageType == e.SourcePageType)
                    {
                        NavView.SelectedItem = item;
                        break;
                    }
                }
            }
        }
    }
}