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

            var toastNotificationsService = Container.Resolve<IToastNotificationsService>();
            toastNotificationsService.ShowToastNotificationSample();

            if (_startUpArgs.Contains(DesktopNotificationManagerCompat.ToastActivatedLaunchArg))
            {
                // ToastNotificationActivator code will run after this completes and will show a window if necessary.
                return;
            }

            await Task.CompletedTask;
            base.OnInitialized();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _startUpArgs = e.Args;
            base.OnStartup(e);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Core Services
            PrismContainerExtension.Create(Container.GetContainer());
            containerRegistry.Register<IFileService, FileService>();

            // App Services
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
