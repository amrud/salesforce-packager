using MahApps.Metro.Controls;
using Squirrel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SalesforcePackager
{
    /// <summary>
    /// Interaction logic for AboutUserControl.xaml
    /// </summary>
    public partial class AboutUserControl : UserControl
    {
        private Flyout flyout;
        private bool isRestart = false;
        private string latestExe = "";

        public AboutUserControl()
        {
            InitializeComponent();
            try
            {
                lblHeader.Content = Assembly.GetExecutingAssembly().GetName().Name.ToString();
                lblVersion.Content += Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Width = 0.85 * Application.Current.MainWindow.Width;
            }
            catch (Exception)
            {

            }
            var mainWindow = (Application.Current.MainWindow as MetroWindow);
            Flyout item = mainWindow.Flyouts.Items[1] as Flyout;

            if (item != null)
            {
                flyout = item;
                flyout.IsOpenChanged += Flyout_IsOpenChanged;
            }
        }

        private async void Flyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (flyout != null && flyout.IsOpen)
            {

                try
                {
                    Width = 0.85 * Application.Current.MainWindow.Width;

                    toggleStatusIcon(true);
                    

                    if (!string.IsNullOrEmpty(Common.Settings.getAppCache().updaterPath))
                    {
                        using (var mgr = new UpdateManager(Common.Settings.getAppCache().updaterPath))
                        {
                            ReleaseEntry re = await mgr.UpdateApp();
                            toggleStatusIcon(false, MahApps.Metro.IconPacks.PackIconMaterialKind.CheckCircle);
                            lblStatus.Content = "Up-to-date";
                            if (re != null)
                            {
                                lblVersion.Content = re.Version;
                            }
                        }
                    }
                    else
                    {
                        toggleStatusIcon(false, MahApps.Metro.IconPacks.PackIconMaterialKind.CheckCircle);
                        //toggleStatusIcon(false, MahApps.Metro.IconPacks.PackIconMaterialKind.ServerNetworkOff);
                        lblStatus.Content = "Packager is up to date";
                    }
                }
                catch (Exception ex)
                {
                    toggleStatusIcon(false, MahApps.Metro.IconPacks.PackIconMaterialKind.CloseCircle);
                    lblStatus.Content = ex.Message; 
                }
            }
        }
         
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (isRestart)
            {
                UpdateManager.RestartApp(latestExe);
            }
            else
            {
                if (flyout != null)
                {
                    flyout.IsOpen = false;
                }
            }
        }

        private void toggleStatusIcon(bool showProgress, MahApps.Metro.IconPacks.PackIconMaterialKind icon = MahApps.Metro.IconPacks.PackIconMaterialKind.CloseCircle)
        {
            pbLoading.IsActive = showProgress;

            if (showProgress)
            {
                ipStatus.Visibility = Visibility.Collapsed;   
            }
            else
            {
                ipStatus.Visibility = Visibility.Visible;
                ipStatus.Kind = icon;
            }
        }

         
    }
}
