using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BasicApp.Contracts.Activation;
using BasicApp.Contracts.Services;
using BasicApp.Contracts.Views;
using BasicApp.Models;
using BasicApp.ViewModels;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BasicApp.Services
{
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IActivationHandler> _activationHandlers;
        private readonly INavigationService _navigationService;
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly IThemeSelectorService _themeSelectorService;
        private readonly AppConfig _appConfig;
        private readonly IToastNotificationsService _toastNotificationsService;

        private bool _isInitialized;
        private IShellWindow _shellWindow;

        public ApplicationHostService(IServiceProvider serviceProvider, IEnumerable<IActivationHandler> activationHandlers, INavigationService navigationService, IThemeSelectorService themeSelectorService, IPersistAndRestoreService persistAndRestoreService, IOptions<AppConfig> appConfig, IToastNotificationsService toastNotificationsService)
        {
            _serviceProvider = serviceProvider;
            _activationHandlers = activationHandlers;
            _navigationService = navigationService;
            _themeSelectorService = themeSelectorService;
            _persistAndRestoreService = persistAndRestoreService;
            _appConfig = appConfig.Value;
            _toastNotificationsService = toastNotificationsService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Initialize services that you need before app activation
            await InitializeAsync();

            await HandleActivationAsync();

            // Tasks after activation
            await StartupAsync();
            _isInitialized = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _persistAndRestoreService.PersistData();
            await Task.CompletedTask;
        }

        private async Task InitializeAsync()
        {
            if (!_isInitialized)
            {
                _persistAndRestoreService.RestoreData();
                _themeSelectorService.SetTheme();
                await Task.CompletedTask;
            }
        }

        private async Task StartupAsync()
        {
            if (!_isInitialized)
            {
                _toastNotificationsService.ShowToastNotificationSample();
                await Task.CompletedTask;
            }
        }        

        private async Task HandleActivationAsync()
        {            
            var activationHandler = _activationHandlers
                                        .FirstOrDefault(h => h.CanHandle());

            if (activationHandler != null)
            {
                await activationHandler.HandleAsync();
            }

            if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
            {
                // Default activation
                _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
                _navigationService.Initialize(_shellWindow.GetNavigationFrame());
                _shellWindow.ShowWindow();
                _navigationService.NavigateTo(typeof(MainViewModel).FullName);                
            }
        }
    }
}
