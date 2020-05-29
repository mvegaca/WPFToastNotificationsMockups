using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using LightApp.Core.Contracts.Services;
using LightApp.Core.Helpers;
using LightApp.Properties;

namespace LightApp.ViewModels
{
    public class LogInViewModel : ViewModelBase
    {
        private readonly IIdentityService _identityService;
        private string _statusMessage;
        private bool _isBusy;
        private RelayCommand _loginCommand;

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                Set(ref _isBusy, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand LoginCommand => _loginCommand ?? (_loginCommand = new RelayCommand(OnLogin, () => !IsBusy));

        public LogInViewModel(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        private async void OnLogin()
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            var loginResult = await _identityService.LoginAsync();
            StatusMessage = GetStatusMessage(loginResult);
            IsBusy = false;
        }

        private string GetStatusMessage(LoginResultType loginResult)
        {
            switch (loginResult)
            {
                case LoginResultType.Unauthorized:
                    return Resources.StatusUnauthorized;
                case LoginResultType.NoNetworkAvailable:
                    return Resources.StatusNoNetworkAvailable;
                case LoginResultType.UnknownError:
                    return Resources.StatusLoginFails;
                case LoginResultType.Success:
                case LoginResultType.CancelledByUser:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }
    }
}
