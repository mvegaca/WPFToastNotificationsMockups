using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using LightApp.Contracts.Activation;
using LightApp.Contracts.Services;
using LightApp.Contracts.Views;
using LightApp.ViewModels;
using Microsoft.Extensions.Configuration;

namespace LightApp.Activation
{
    public class ToastNotificationActivationHandler : IActivationHandler
    {
        public const string ActivationArguments = "ToastNotificationActivationArguments";

        private readonly IConfiguration _config;
        private readonly INavigationService _navigationService;

        public ToastNotificationActivationHandler(IConfiguration config, INavigationService navigationService)
        {
            _config = config;
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
                var shellWindow = SimpleIoc.Default.GetInstance<IShellWindow>();
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
