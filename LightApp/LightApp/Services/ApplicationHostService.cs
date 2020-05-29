using System;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Ioc;

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
        private readonly AppConfig _config;
        private IShellWindow _shellWindow;
        private ILogInWindow _logInWindow;

        public ApplicationHostService(INavigationService navigationService, IThemeSelectorService themeSelectorService, IPersistAndRestoreService persistAndRestoreService, IIdentityService identityService, IUserDataService userDataService, AppConfig config)
        {
            _navigationService = navigationService;
            _themeSelectorService = themeSelectorService;
            _persistAndRestoreService = persistAndRestoreService;
            _identityService = identityService;
            _userDataService = userDataService;
            _config = config;
        }

        public async Task StartAsync()
        {
            // Initialize services that you need before app activation
            await InitializeAsync();

            _identityService.InitializeWithAadAndPersonalMsAccounts(_config.IdentityClientId, "http://localhost");
            var silentLoginSuccess = await _identityService.AcquireTokenSilentAsync();
            if (!silentLoginSuccess || !_identityService.IsAuthorized())
            {
                _logInWindow = SimpleIoc.Default.GetInstance<ILogInWindow>();
                _logInWindow.ShowWindow();
                await StartupAsync();
                return;
            }

            _shellWindow = SimpleIoc.Default.GetInstance<IShellWindow>();
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _shellWindow.ShowWindow();
            _navigationService.NavigateTo(typeof(MainViewModel).FullName);

            // Tasks after activation
            await StartupAsync();
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
            _persistAndRestoreService.RestoreData();
            _themeSelectorService.SetTheme();
            await Task.CompletedTask;
            _userDataService.Initialize();
            _identityService.LoggedIn += OnLoggedIn;
            _identityService.LoggedOut += OnLoggedOut;
        }

        private async Task StartupAsync()
        {
            await Task.CompletedTask;
        }

        private void OnLoggedIn(object sender, EventArgs e)
        {
            _shellWindow = SimpleIoc.Default.GetInstance<IShellWindow>(Guid.NewGuid().ToString());
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _shellWindow.ShowWindow();
            _navigationService.NavigateTo(typeof(MainViewModel).FullName);
            _logInWindow.CloseWindow();
            _logInWindow = null;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            // Show the LogIn Window
            _logInWindow = SimpleIoc.Default.GetInstance<ILogInWindow>(Guid.NewGuid().ToString());
            _logInWindow.ShowWindow();

            // Close the Shell Window and
            _shellWindow.CloseWindow();
            _navigationService.UnsubscribeNavigation();
        }
    }
}
