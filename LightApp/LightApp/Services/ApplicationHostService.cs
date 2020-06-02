using System;
using System.Linq;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Ioc;
using LightApp.Activation;
using LightApp.Contracts.Activation;
using LightApp.Contracts.Services;
using LightApp.Contracts.Views;
using LightApp.Core.Contracts.Services;
using LightApp.Models;
using LightApp.ViewModels;

namespace LightApp.Services
{
    public class ApplicationHostService : IApplicationHostService
    {
        private readonly INavigationService _navigationService;
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private readonly IThemeSelectorService _themeSelectorService;
        private readonly IIdentityService _identityService;
        private readonly IUserDataService _userDataService;
        private readonly IToastNotificationsService _toastNotificationsService;
        private readonly AppConfig _appConfig;
        
        private bool _isInitialized;
        private IShellWindow _shellWindow;
        private ILogInWindow _logInWindow;

        public ApplicationHostService(INavigationService navigationService, IThemeSelectorService themeSelectorService, IPersistAndRestoreService persistAndRestoreService, IIdentityService identityService, IUserDataService userDataService, AppConfig appConfig, IToastNotificationsService toastNotificationsService)
        {
            _navigationService = navigationService;
            _themeSelectorService = themeSelectorService;
            _persistAndRestoreService = persistAndRestoreService;
            _identityService = identityService;
            _userDataService = userDataService;
            _appConfig = appConfig;
            _toastNotificationsService = toastNotificationsService;
        }

        public async Task StartAsync()
        {
            // Initialize services that you need before app activation
            await InitializeAsync();

            _identityService.InitializeWithAadAndPersonalMsAccounts(_appConfig.IdentityClientId, "http://localhost");
            var silentLoginSuccess = await _identityService.AcquireTokenSilentAsync();
            if (!silentLoginSuccess || !_identityService.IsAuthorized())
            {
                _logInWindow = SimpleIoc.Default.GetInstance<ILogInWindow>();
                _logInWindow.ShowWindow();
                await StartupAsync();
                return;
            }

            await HandleActivationAsync();

            // Tasks after activation
            await StartupAsync();
            _isInitialized = true;
        }

        public async Task StopAsync()
        {
            _persistAndRestoreService.PersistData();
            await Task.CompletedTask;
            _identityService.LoggedIn -= OnLoggedIn;
            _identityService.LoggedOut -= OnLoggedOut;
        }

        private async Task InitializeAsync()
        {
            if (!_isInitialized)
            {
                _persistAndRestoreService.RestoreData();
                _themeSelectorService.SetTheme();
                await Task.CompletedTask;
                _userDataService.Initialize();
                _identityService.LoggedIn += OnLoggedIn;
                _identityService.LoggedOut += OnLoggedOut;
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

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            await HandleActivationAsync();
            _logInWindow.CloseWindow();
            _logInWindow = null;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            _logInWindow = SimpleIoc.Default.GetInstance<ILogInWindow>(Guid.NewGuid().ToString());
            _logInWindow.ShowWindow();

            _shellWindow.CloseWindow();
            _navigationService.UnsubscribeNavigation();
        }

        private async Task HandleActivationAsync()
        {
            var activationHandler = SimpleIoc.Default.GetAllInstances<IActivationHandler>()
                                        .FirstOrDefault(h => h.CanHandle());

            if (activationHandler != null)
            {
                await activationHandler.HandleAsync();
            }
            await Task.CompletedTask;

            if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
            {
                // Default activation
                _shellWindow = SimpleIoc.Default.GetInstance<IShellWindow>();
                _navigationService.Initialize(_shellWindow.GetNavigationFrame());
                _shellWindow.ShowWindow();
                _navigationService.NavigateTo(typeof(MainViewModel).FullName);
            }
        }
    }
}
