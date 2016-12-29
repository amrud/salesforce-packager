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
using MahApps.Metro.Controls;
using SalesforcePackager.Metadata;
using System.IO;
using System.Net;
using System;
using System.Threading;
using SalesforcePackager.Model;
using Newtonsoft.Json;
using SalesforcePackager.Views;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using ToastNotifications;

namespace SalesforcePackager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    { 
        public MainWindow()
        {
            InitializeComponent();
            RightWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;

            List<Instance> instances = Common.Settings.getAppCache().instanceList;


            foreach (var item in instances)
            {
                InstanceDetailsUserControl iduc = new InstanceDetailsUserControl();
                iduc._instanceOrgId = item.instanceOrgId;
                iduc.InitTree();
                // add a tabItem with + in header 
                TabItem tabAdd = new TabItem();
                tabAdd.Header = item.instanceName;
                tabAdd.Content = iduc;
                tabControl.Items.Add(tabAdd);
            }
        }


        #region Setting/About
        private void OpenSettingPage()
        {
            var flyout = base.Flyouts.Items[0] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }

        private void btnSetting_Clicked(object sender, RoutedEventArgs e)
        {
            var flyout = base.Flyouts.Items[0] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.Width = 0.9 * Application.Current.MainWindow.Width;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void btnAbout_Clicked(object sender, RoutedEventArgs e)
        {
            var flyout = base.Flyouts.Items[1] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.Width = 0.9 * Application.Current.MainWindow.Width;
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void btnTool_Click(object sender, RoutedEventArgs e)
        {
            var flyout = base.Flyouts.Items[2] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.Width = 0.9 * Application.Current.MainWindow.Width;
            flyout.IsOpen = !flyout.IsOpen;
        }

        #endregion

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        
    }
}
