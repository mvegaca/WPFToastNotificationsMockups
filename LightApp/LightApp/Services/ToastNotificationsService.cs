using LightApp.Contracts.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace LightApp.Services
{
    public partial class ToastNotificationsService : IToastNotificationsService
    {
        public ToastNotificationsService()
        {
        }

        public void ShowToastNotification(ToastNotification toastNotification)
        {
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
        }
    }
}
