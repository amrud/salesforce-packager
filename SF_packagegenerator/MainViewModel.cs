using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications;

namespace SalesforcePackager
{
    class MainViewModel : INotifyPropertyChanged
    {
        private NotificationsSource _notificationSource;

        public NotificationsSource NotificationSource
        {
            get { return _notificationSource; }
            set
            {
                _notificationSource = value;
                OnPropertyChanged("NotificationSource");
            }
        }

        public MainViewModel()
        {
            NotificationSource = new NotificationsSource()
            {
                MaximumNotificationCount = 4,
                NotificationLifeTime = TimeSpan.FromSeconds(3)
            };
        }

        public void ShowInformation(string message)
        {
            NotificationSource.Show(message, NotificationType.Information);
        }

        public void ShowSuccess(string message)
        {
            NotificationSource.Show(message, NotificationType.Success);
        }

        public void ShowWarning(string message)
        {
            NotificationSource.Show(message, NotificationType.Warning);
        }

        public void ShowError(string message)
        {
            NotificationSource.Show(message, NotificationType.Error);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
