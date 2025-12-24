using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using myshop.Services;
using myshop.ViewModels;
using myshop_data.Data;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace myshop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private static ServiceProvider? _serviceProvider;

        /// <summary>
        /// Service provider để resolve dependencies
        /// </summary>
        public static ServiceProvider? ServiceProvider => _serviceProvider;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            ConfigureServices();
     
            // ✅ Bắt global unhandled exceptions
            this.UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// Configure Dependency Injection
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<NavigationService>();
            services.AddSingleton<DatabaseConfigService>();
            services.AddSingleton<DatabaseTestService>();
            
            // Đăng ký DbContextService thay vì DbContextFactory
            // Service này sẽ lazy load connection string khi cần
            services.AddSingleton<DbContextService>();

            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ConfigViewModel>();
            services.AddTransient<ProductsViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new LoginWindow();
            _window.Activate();
        }

        // ✅ Xử lý global unhandled exceptions
        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Log exception nhưng không crash app
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.Exception?.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {e.Exception?.StackTrace}");
            
            // Nếu là exception do cancel hoặc dispose, ignore
            if (e.Exception is OperationCanceledException || 
                e.Exception is ObjectDisposedException ||
                e.Exception?.InnerException is OperationCanceledException ||
                e.Exception?.InnerException is ObjectDisposedException)
            {
                e.Handled = true;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CurrentDomain unhandled exception: {ex.Message}");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Unobserved task exception: {e.Exception?.Message}");
   
            // Mark as observed để không crash app
            e.SetObserved();
        }
    }
}
