using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using SalesforcePackager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace SalesforcePackager
{
    /// <summary>
    /// Interaction logic for SettingPage.xaml
    /// </summary>
    public partial class SettingUserControl : UserControl
    {
        public List<Instance> instanceList;
        private ApplicationData appData;
        private Flyout flyout;

        public SettingUserControl()
        {
            InitializeComponent();
            try
            {
                Width = 0.85 * Application.Current.MainWindow.Width;
            }
            catch (Exception)
            {

            }
            var mainWindow = (Application.Current.MainWindow as MetroWindow);
            Flyout item = mainWindow.Flyouts.Items[0] as Flyout;

            if (item != null)
            {
                flyout = item;
                flyout.IsOpenChanged += Flyout_IsOpenChanged;
            }
        }      
                  
        private void Flyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (flyout != null && flyout.IsOpen)
            {
                try
                {
                    Width = 0.85 * Application.Current.MainWindow.Width;

                    appData = Common.Settings.getAppCache();
                    instanceList = appData.instanceList;
                    dataGrid.ItemsSource = instanceList;

                    txtBoxDefaultPath.Text = appData.defaultProjectPath;
                    txtBoxDefaultUpdaterPath.Text = appData.updaterPath;

                    btnDelete.IsEnabled = false;
                    btnEdit.IsEnabled = false;
                }
                catch (Exception)
                {

                }
            }
        }

        private void btnNewInstance_Click(object sender, RoutedEventArgs e)
        {
            openNewInstanceDialog();
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Instance instance = dataGrid.SelectedItem as Instance;
            if (instance != null)
            {
                btnDelete.IsEnabled = true;
                btnEdit.IsEnabled = true;
            }else
            {
                btnDelete.IsEnabled = false;
                btnEdit.IsEnabled = false;
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            MessageDialogResult confirmDeleteDialog = await metroWindow.ShowMessageAsync("Remove Confirmation", "This will remove the instance", MessageDialogStyle.AffirmativeAndNegative);
            if (confirmDeleteDialog == MessageDialogResult.Affirmative)
            {
                int index= dataGrid.SelectedIndex;
                instanceList.RemoveAt(index);
                dataGrid.ItemsSource = null;
                dataGrid.ItemsSource = instanceList;
                appData.instanceList = instanceList;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Instance instance = dataGrid.SelectedItem as Instance;
            openNewInstanceDialog(instance);
        }

        private void openNewInstanceDialog(Instance i = null)
        {

            NewInstance newInstance = new NewInstance(i);
            if (i == null)
            {
                newInstance = new NewInstance();
            }
            newInstance.Closed += (s, evetArg) =>
            {
                dataGrid.ItemsSource = null;
                appData = Common.Settings.getAppCache();
                instanceList = appData.instanceList;
                dataGrid.ItemsSource = instanceList;
            };

            newInstance.ShowDialog();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Common.Settings.updateAppCache(appData);
            if (flyout != null)
            {
                flyout.IsOpen = false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (flyout != null)
            {
                flyout.IsOpen = false;
            }
        }

        private async void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog vofd = new VistaFolderBrowserDialog();
            vofd.Description = "Select project directory";
            vofd.UseDescriptionForTitle = true;
            vofd.ShowNewFolderButton = true;
            vofd.RootFolder = Environment.SpecialFolder.MyDocuments;
            var file = vofd.ShowDialog();
            if (file == true)
            {
                string path = vofd.SelectedPath;

                try
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        appData.defaultProjectPath = path;
                        txtBoxDefaultPath.Text = path;
                    }
                    else
                    {
                        throw new Exception();
                    }

                }
                catch (Exception)
                {
                    var metroWindow = (Application.Current.MainWindow as MetroWindow);
                    await metroWindow.ShowMessageAsync("Invalid Directory", "Please select a valid directory");
                }

            }
        }

        private async void btnBrowseUpdaterPath_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog vofd = new VistaFolderBrowserDialog();
            vofd.Description = "Select updater directory";
            vofd.UseDescriptionForTitle = true;
            vofd.ShowNewFolderButton = true;
            vofd.RootFolder = Environment.SpecialFolder.MyDocuments;
            var file = vofd.ShowDialog();
            if (file == true)
            {
                string path = vofd.SelectedPath;

                try
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        appData.updaterPath = path;
                        txtBoxDefaultUpdaterPath.Text = path;
                    }
                    else
                    {
                        throw new Exception();
                    }

                }
                catch (Exception)
                {
                    var metroWindow = (Application.Current.MainWindow as MetroWindow);
                    await metroWindow.ShowMessageAsync("Invalid Directory", "Please select a valid directory");
                }

            }
        }

        private void txtBoxDefaultUpdaterPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            appData.updaterPath = txtBoxDefaultUpdaterPath.Text;
        }
    }
}
