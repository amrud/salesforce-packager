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
using System.Windows.Shapes;

namespace SalesforcePackager
{
    /// <summary>
    /// Interaction logic for NewDeployment.xaml
    /// </summary>
    public partial class NewDeployment : MetroWindow
    {

        private Project deploymentProject;
        private List<Instance> instanceSettings;

        public NewDeployment(string instanceName)
        {
            InitializeComponent();
            instanceSettings = Common.Settings.getInstanceSettings();
            if (instanceSettings != null)
            {
                instanceSettings.Insert(0, new Instance() { instanceName = "Select Instance" });
            }
            comboBoxInstances.ItemsSource = instanceSettings;
            //comboBoxInstances.SelectedIndex = 0;
            comboBoxInstances.IsEnabled = false;
            for(int x = 0; x < instanceSettings.Count; x++)
            {
                if (instanceSettings[x].instanceOrgId == instanceName)
                {
                    comboBoxInstances.SelectedIndex = x;
                    break;
                }
            }

            deploymentProject = new Project();
            lblProjectPath.Text = Common.Settings.getAppCache().defaultProjectPath;
            lblProjectPath.ToolTip = Common.Settings.getAppCache().defaultProjectPath;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtboxName.Text) || comboBoxInstances.SelectedIndex == 0 || string.IsNullOrEmpty(txtBoxProjectPath.Text))
            {
                return;
            }

            createNewDeployment();
            this.Close();
        }
         
        private void createNewDeployment()
        {

            Instance inst = (Instance)comboBoxInstances.SelectedItem;
             
            deploymentProject.sfInstance = inst.instanceName;
            deploymentProject.sfUrl = inst.instanceUrl;
            deploymentProject.sfInstanceOrgId = inst.instanceOrgId;
            deploymentProject.components = new List<Metadata.FileProperties>();
            deploymentProject.projectPath = string.Format(@"{0}\{1}", lblProjectPath.LongText, txtBoxProjectPath.Text);
            deploymentProject.name = txtboxName.Text;
            deploymentProject.createdDate = DateTime.Now;

            Common.Utility.saveProjectData(deploymentProject);
            ApplicationData appData = Common.Settings.getAppCache();
            appData.recentProjects.Insert(0, deploymentProject);
            Common.Settings.updateAppCache(appData);
        }

        private void txtboxName_Error(object sender, ValidationErrorEventArgs e)
        {

        }

        private void comboBoxInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxInstances.SelectedIndex > 0)
            {
                lblUrl.Text = ((Instance)comboBoxInstances.SelectedItem).instanceUrl;
            }
           
        }

        private void txtboxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBoxProjectPath.Text = txtboxName.Text.Replace(" ","");
        }
         
        private async void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog vofd = new VistaFolderBrowserDialog();
            vofd.Description = "Select project directory";
            vofd.UseDescriptionForTitle = true;
            vofd.ShowNewFolderButton = true;
            vofd.SelectedPath = Common.Settings.getAppCache().defaultProjectPath;
            var file = vofd.ShowDialog();
            if (file == true)
            {
                string path = vofd.SelectedPath;

                try
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        lblProjectPath.Text = path;
                        lblProjectPath.ToolTip = path;
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
    }
}
