using Microsoft.Toolkit.Uwp.Notifications;
using PrismApp.Contracts.Services;
using Windows.UI.Notifications;

namespace PrismApp.Services
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
