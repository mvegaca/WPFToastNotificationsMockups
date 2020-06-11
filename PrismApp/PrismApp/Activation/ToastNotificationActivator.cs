using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Uwp.Notifications;

namespace PrismApp.Activation
{
    // The GUID CLSID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("07FDB068-68A5-4D64-BB45-A9404468742D"), ComVisible(true)]
    public class ToastNotificationActivator : NotificationActivator
    {
        public override async void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                var app = Application.Current as App;
                var config = app.Resolve<IConfiguration>();
                // Store ToastNotification arguments in configuration, so you can use them from any point in the app
                config[App.ToastNotificationActivationArguments] = arguments;

                Application.Current.MainWindow.Show();
                App.Current.MainWindow.Activate();
                if (App.Current.MainWindow.WindowState == WindowState.Minimized)
                {
                    App.Current.MainWindow.WindowState = WindowState.Normal;
                }
                await Task.CompletedTask;
            });
        }
    }
}
