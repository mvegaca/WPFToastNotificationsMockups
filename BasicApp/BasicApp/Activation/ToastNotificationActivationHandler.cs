using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BasicApp.Contracts.Activation;
using BasicApp.Contracts.Services;
using BasicApp.Contracts.Views;
using BasicApp.ViewModels;
using Microsoft.Extensions.Configuration;

namespace BasicApp.Activation
{
    public class ToastNotificationActivationHandler : IActivationHandler
    {
        public const string ActivationArguments = "ToastNotificationActivationArguments";

        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;

        public ToastNotificationActivationHandler(IConfiguration config, IServiceProvider serviceProvider, INavigationService navigationService)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
        }

        public bool CanHandle()
        {
            return !string.IsNullOrEmpty(_config[ActivationArguments]);
        }

        public async Task HandleAsync()
        {
            if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
            {
                var shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
                _navigationService.Initialize(shellWindow.GetNavigationFrame());
                shellWindow.ShowWindow();
                _navigationService.NavigateTo(typeof(SettingsViewModel).FullName);
            }
            else
            {
                App.Current.MainWindow.Activate();
                if (App.Current.MainWindow.WindowState == WindowState.Minimized)
                {
                    App.Current.MainWindow.WindowState = WindowState.Normal;
                }

                _navigationService.NavigateTo(typeof(SettingsViewModel).FullName);
            }

            await Task.CompletedTask;
        }
    }
}
