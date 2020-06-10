using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using PrismApp.Activation;
using PrismApp.Constants;
using PrismApp.Contracts.Services;
using PrismApp.Core.Contracts.Services;
using PrismApp.Core.Services;
using PrismApp.Models;
using PrismApp.Services;
using PrismApp.ViewModels;
using PrismApp.Views;

namespace PrismApp
{
    public partial class App : PrismApplication
    {
        public const string ToastNotificationActivationArguments = "ToastNotificationActivationArguments";

        private LogInWindow _logInWindow;

        private string[] _startUpArgs;

        public App()
        {
        }

        public T Resolve<T>() where T : class
            => Container.Resolve<T>();

        protected override Window CreateShell()
            => Container.Resolve<ShellWindow>();

        protected override async void OnInitialized()
        {
            // Read more about sending local toast notifications from desktop C# apps
            // https://docs.microsoft.com/windows/uwp/design/shell/tiles-and-notifications/send-local-toast-desktop
            //
            // Register AUMID, COM server, and activator
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<ToastNotificationActivator>("PrismApp");
            DesktopNotificationManagerCompat.RegisterActivator<ToastNotificationActivator>();
            var persistAndRestoreService = Container.Resolve<IPersistAndRestoreService>();
            persistAndRestoreService.RestoreData();

            var themeSelectorService = Container.Resolve<IThemeSelectorService>();
            themeSelectorService.SetTheme();

            var userDataService = Container.Resolve<IUserDataService>();
            userDataService.Initialize();

            var appConfig = Container.Resolve<AppConfig>();
            var identityService = Container.Resolve<IIdentityService>();
            identityService.InitializeWithAadAndPersonalMsAccounts(appConfig.IdentityClientId, "http://localhost");
            identityService.LoggedIn += OnLoggedIn;
            identityService.LoggedOut += OnLoggedOut;

            var toastNotificationsService = Container.Resolve<IToastNotificationsService>();
            toastNotificationsService.ShowToastNotificationSample();

            var silentLoginSuccess = await identityService.AcquireTokenSilentAsync();
            if (!silentLoginSuccess || !identityService.IsAuthorized())
            {
                ShowLogInWindow();
                return;
            }

            if (_startUpArgs.Contains(DesktopNotificationManagerCompat.ToastActivatedLaunchArg))
            {
                // ToastNotificationActivator code will run after this completes and will show a window if necessary.
                return;
            }

            base.OnInitialized();
        }

        private void OnLoggedIn(object sender, EventArgs e)
        {
            if (!(Application.Current.MainWindow is ShellWindow))
            {
                Application.Current.MainWindow = CreateShell();
                RegionManager.UpdateRegions();
            }

            Application.Current.MainWindow.Show();
            _logInWindow.Close();
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            ShowLogInWindow();
            Application.Current.MainWindow.Close();
        }

        private void ShowLogInWindow()
        {
            _logInWindow = Container.Resolve<LogInWindow>();
            _logInWindow.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _startUpArgs = e.Args;
            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Core Services
            containerRegistry.Register<IMicrosoftGraphService, MicrosoftGraphService>();

            PrismContainerExtension.Create(Container.GetContainer());
            PrismContainerExtension.Current.RegisterServices(s =>
            {
                s.AddHttpClient("msgraph", client =>
                {
                    client.BaseAddress = new System.Uri("https://graph.microsoft.com/v1.0/");
                });
            });

            containerRegistry.Register<IIdentityCacheService, IdentityCacheService>();
            containerRegistry.RegisterSingleton<IIdentityService, IdentityService>();
            containerRegistry.Register<IFileService, FileService>();

            // App Services
            containerRegistry.RegisterSingleton<IUserDataService, UserDataService>();
            containerRegistry.Register<IApplicationInfoService, ApplicationInfoService>();
            containerRegistry.Register<ISystemService, SystemService>();
            containerRegistry.Register<IPersistAndRestoreService, PersistAndRestoreService>();
            containerRegistry.Register<IThemeSelectorService, ThemeSelectorService>();
            containerRegistry.RegisterSingleton<IToastNotificationsService, ToastNotificationsService>();

            // Views
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>(PageKeys.Settings);
            containerRegistry.RegisterForNavigation<MainPage, MainViewModel>(PageKeys.Main);
            containerRegistry.RegisterForNavigation<ShellWindow, ShellViewModel>();

            // Configuration
            var configuration = BuildConfiguration();
            var appConfig = configuration
                .GetSection(nameof(AppConfig))
                .Get<AppConfig>();

            // Register configurations to IoC
            containerRegistry.RegisterInstance<IConfiguration>(configuration);
            containerRegistry.RegisterInstance<AppConfig>(appConfig);
        }

        private IConfiguration BuildConfiguration()
        {
            // TODO: Register arguments you want to use on App initialization
            var activationArgs = new Dictionary<string, string>
            {
                { ToastNotificationActivationArguments, string.Empty},
            };
            var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return new ConfigurationBuilder()
                .SetBasePath(appLocation)
                .AddJsonFile("appsettings.json")
                .AddCommandLine(_startUpArgs)
                .AddInMemoryCollection(activationArgs)
                .Build();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            var persistAndRestoreService = Container.Resolve<IPersistAndRestoreService>();
            persistAndRestoreService.PersistData();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/dotnet/api/system.windows.application.dispatcherunhandledexception?view=netcore-3.0

            // e.Handled = true;
        }
    }
}
