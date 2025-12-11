using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using myshop.Services;
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

            // DbContext will be registered later when we have connection string
            // For now, we'll create it on-demand in services

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
