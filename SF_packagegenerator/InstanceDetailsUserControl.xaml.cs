using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using SalesforcePackager.Model;
using SalesforcePackager.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for InstanceDetailsUserControl.xaml
    /// </summary>
    public partial class InstanceDetailsUserControl : UserControl
    {
        public string _instanceOrgId { get; set; }
        private readonly MainViewModel toast;

        public InstanceDetailsUserControl()
        {
            InitializeComponent();

            DataContext = toast = new MainViewModel();
        }

        public void InitTree()
        {
            treeView.Items.Clear();
            List<Project> recentProjects = Common.Settings.getAppCache().recentProjects.Where(x => x.sfInstanceOrgId == _instanceOrgId).ToList();

            foreach (Project project in recentProjects)
            {
                TreeViewMenuItem treeViewItem = new TreeViewMenuItem();
                treeViewItem.title = project.name;
                treeViewItem.project = project;
                treeView.Items.Add(treeViewItem);
            }


        }

        private async void removeProjectFromRecent(string title, string message, TreeViewMenuItem sender, MessageDialogStyle dialogStyle)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            MessageDialogResult confirmDeleteDialog = await metroWindow.ShowMessageAsync(title, message, dialogStyle);
            if (confirmDeleteDialog == MessageDialogResult.Affirmative)
            {
                ApplicationData appData = Common.Settings.getAppCache();
                TreeViewMenuItem tvmi = sender;
                Project p = tvmi.project;
                appData.recentProjects.Remove(p);

                foreach (var item in appData.recentProjects)
                {
                    if (item.name.Equals(p.name))
                    {
                        appData.recentProjects.Remove(item);
                        break;
                    }
                }

                Common.Settings.updateAppCache(appData);
                InitTree();
            }
        }

        private void menuShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            MenuItem x = sender as MenuItem;
            TreeViewMenuItem tvmi = x.DataContext as TreeViewMenuItem;
            Project p = tvmi.project;
            try
            {
                // opens the folder in explorer
                Process.Start(p.projectPath);
            }
            catch (Exception)
            {
                // opens the folder in explorer
                Process.Start(@"c:\");
            }
        }

        private void btnProjectMenuOptionRemove_Clicked(object sender, RoutedEventArgs e)
        {
            MenuItem x = sender as MenuItem;
            removeProjectFromRecent("Remove Confirmation", "This will remove the project from the recent list", x.DataContext as TreeViewMenuItem, MessageDialogStyle.AffirmativeAndNegative);
        }

        private async void btnProjectMenuOptionDelete_Clicked(object sender, RoutedEventArgs e)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            MessageDialogResult confirmDeleteDialog = await metroWindow.ShowMessageAsync("Delete Confirmation", "This will delete all related project files", MessageDialogStyle.AffirmativeAndNegative);

            if (confirmDeleteDialog == MessageDialogResult.Affirmative)
            {
                MenuItem x = sender as MenuItem;
                TreeViewMenuItem tvmi = x.DataContext as TreeViewMenuItem;
                Project p = tvmi.project;

                if (Directory.Exists(p.projectPath))
                {
                    Common.Utility.DeleteDirectory(p.projectPath);
                }

                ApplicationData appData = Common.Settings.getAppCache();
                foreach (var item in appData.recentProjects)
                {
                    if (item.name.Equals(p.name))
                    {
                        appData.recentProjects.Remove(item);
                        break;
                    }
                }

                Common.Settings.updateAppCache(appData);
                InitTree();
            }
        }

        private bool CheckInstanceSettings()
        {
            return Common.Settings.getInstanceSettings().Any();
        }
        
        private async void btnNew_Clicked(object sender, RoutedEventArgs e)
        {
            if (CheckInstanceSettings())
            {
                //open new window for deployment details
                //ok button to create 
                //cancel to close
                NewDeployment newDeployment = new NewDeployment(_instanceOrgId);
                newDeployment.Owner = Application.Current.MainWindow;
                newDeployment.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                newDeployment.Closed += (s, evetArg) =>
                {
                    InitTree();
                };
                newDeployment.ShowDialog();
            }
            else
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("No instance available", "Go to the Settings > Salesforce Instances and add a new Salesforce instance");
            }
        }

        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog vofd = new VistaOpenFileDialog();
            vofd.Filter = "JSON files |*.json";
            vofd.Title = "Open Existing Project";
            vofd.Multiselect = false;
            vofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var file = vofd.ShowDialog();
            if (file == true)
            {
                string fileName = vofd.FileName;

                try
                {
                    Project project = Common.Utility.readProjectData(fileName);
                    if (project != null)
                    {
                        ApplicationData appData = Common.Settings.getAppCache();
                        if (!appData.recentProjects.Exists(x => x.projectPath == project.projectPath))
                        {
                            appData.recentProjects.Insert(0, project);
                            Common.Settings.updateAppCache(appData);
                            InitTree();
                        }
                    }
                    else


                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    var metroWindow = (Application.Current.MainWindow as MetroWindow);
                    await metroWindow.ShowMessageAsync("Invalid Project File", "The file you are opening is not a valid ANT generator project file");
                }

            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewMenuItem x = e.NewValue as TreeViewMenuItem;
            ClearHistory();
            if (x != null)
            {
                frameContent.Content = null;

                Project project = Common.Utility.readProjectData(x.project.dataPath);

                if (project != null)
                {
                    DeploymentDetails deploymentDetails = new DeploymentDetails(project);
                    deploymentDetails.toastHandler += DeploymentDetails_toastHandler;
                    frameContent.Content = deploymentDetails;
                }
                else
                {
                    removeProjectFromRecent("Project Not Found", "Project will be removed as no longer exist", x, MessageDialogStyle.Affirmative);
                }

                imageEmpty.Visibility = Visibility.Hidden;
            }
            else
            {
                frameContent.Content = null;
                imageEmpty.Visibility = Visibility.Visible;
            }
        }

        private void ClearHistory()
        {
            if (!frameContent.CanGoBack && !frameContent.CanGoForward)
            {
                return;
            }
            var entry = frameContent.RemoveBackEntry();
            while (entry != null)
            {
                entry = frameContent.RemoveBackEntry();
            }
            frameContent.Navigate(new PageFunction<string>() { RemoveFromJournal = true });
        }

        private void DeploymentDetails_toastHandler(object sender, EventArgs e)
        {
            string message = "Package generated";
            toast.ShowInformation(message);
        }

    }
}
