using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using SalesforcePackager.Model;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using SalesforcePackager.Metadata;
using SalesforcePackager.SFDC;

namespace SalesforcePackager
{
    /// <summary>
    /// Interaction logic for NewInstance.xaml
    /// </summary>
    public partial class NewInstance : MetroWindow
    {
        public List<Url> instanceUrls; 
        private Instance instance;
        private Instance oldInstance = null;
        private BackgroundWorker workerGetMetadataList;

        public NewInstance(Instance i = null)
        {
            InitializeComponent();

            instanceUrls = new List<Url>()
            { new Url() { name = "Select endpoint url" }, new Url() { name = "Production", url = "https://login.salesforce.com" }, new Url() { name="Sandbox", url="https://test.salesforce.com" }, new Url() { name = "Custom" , url = ""} };

            comboBoxUrl.ItemsSource = instanceUrls;
            comboBoxUrl.SelectedIndex = 0;

            if (i != null)
            {
                instance = i;
                oldInstance = i;
                txtInstanceName.Text = instance.instanceName;
                txtUserName.Text = instance.instanceUsername;
                txtPassword.Password = instance.instancePassword;
                txtSecToken.Text = instance.instanceToken;

                if (instance.instanceUrl.Equals(instanceUrls[1].url))
                {
                    comboBoxUrl.SelectedIndex = 1;
                }
                else if (instance.instanceUrl.Equals(instanceUrls[2].url))
                {
                    comboBoxUrl.SelectedIndex = 2;
                }
                else
                {
                    comboBoxUrl.SelectedIndex = 3;
                }

                txtCustomUrl.Text = instance.instanceUrl;
                btnCreate.Content = "Save";

                lblTitle.Content = "Update Instance";
                comboBoxUrl.IsEnabled = false;
                txtCustomUrl.IsEnabled = false;
            }

            workerGetMetadataList = new BackgroundWorker();
            workerGetMetadataList.DoWork += WorkerGetMetadataList_DoWork;
            workerGetMetadataList.RunWorkerCompleted += WorkerGetMetadataList_RunWorkerCompleted;

        }
        
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        { 
            if (comboBoxUrl.SelectedIndex == 0 || string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtPassword.Password) || string.IsNullOrEmpty(txtSecToken.Text))
            {
                return;
            }

            instance = new Instance()
            {
                instanceName = txtInstanceName.Text.Trim(),
                instanceUsername = txtUserName.Text.Trim(),
                instancePassword = txtPassword.Password.Trim(),
                instanceToken = txtSecToken.Text.Trim(),
                instanceUrl = txtCustomUrl.Text
            };

            toggleProgress();
            //btnCreate.Content = "Verifying...";
            btnCreate.IsEnabled = false;
            BackgroundWorker workerLogin = new BackgroundWorker();
            workerLogin.DoWork += WorkerLogin_DoWork;
            workerLogin.RunWorkerCompleted += WorkerLogin_RunWorkerCompleted;
            workerLogin.RunWorkerAsync();
             
        }

        private async void WorkerGetMetadataList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnCreate.IsEnabled = true;
            toggleProgress();

            if (instance != null)
            {
                if (oldInstance != null)
                {
                    Common.Settings.saveInstanceSetting(oldInstance, instance);
                }
                else
                {
                    List<Instance> existingInstance = Common.Settings.getAppCache().instanceList;
                    if (!existingInstance.Where(x => x.instanceOrgId == instance.instanceOrgId ).Any())
                    {
                        Common.Settings.addInstanceSetting(instance);
                    }
                    else
                    {
                        await this.ShowMessageAsync("Error", "Similar Instance Exist");
                        return;
                    }
                }

            }

            this.Close();
        }

        private void WorkerGetMetadataList_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DescribeMetadataResult describeMetadataResultCache;

                if (instance.describeMetadataResultCache != null)
                {
                    describeMetadataResultCache = instance.describeMetadataResultCache;
                }
                else
                {
                    describeMetadataResultCache = Operations.getComponentList(instance);
                    instance.describeMetadataResultCache = describeMetadataResultCache;
                    
                }

                List<FileProperties> reportfolderList = SFDC.Operations.getComponentItemsDetails(instance, "ReportFolder");
                instance.reportFolderCache = reportfolderList;

                List<FileProperties> emailFolderList = SFDC.Operations.getComponentItemsDetails(instance, "EmailFolder");
                instance.emailFolderCache = emailFolderList;

                List<FileProperties> documentFolderList = SFDC.Operations.getComponentItemsDetails(instance, "DocumentFolder");
                instance.documentFolderCache = documentFolderList;

                List<FileProperties> dashboardFolderList = SFDC.Operations.getComponentItemsDetails(instance, "DashboardFolder");
                instance.dashboardFolderCache = dashboardFolderList;

                SFDC.GetUserInfoResult userInfo = SFDC.Operations.getUserInfoSample(instance);
                instance.instanceOrgId = userInfo.organizationId;
                instance.instanceOrgName = userInfo.organizationName;
                
            }
            catch (Exception)
            {
               
            }
        }

        private async void WorkerLogin_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoginMessage loginMessage = e.Result as LoginMessage;
            if (loginMessage.isLogin)
            {
                workerGetMetadataList.RunWorkerAsync();
            }
            else
            { 
                await this.ShowMessageAsync("Error", loginMessage.message);
                btnCreate.IsEnabled = true;
                toggleProgress();
            }                       
        }

        private void WorkerLogin_DoWork(object sender, DoWorkEventArgs e)
        {
            //verify first
            LoginMessage loginMessage = SFDC.Login.login(instance);
            e.Result = loginMessage;        
        }
         
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void toggleProgress()
        {
            if (pbar.Visibility == Visibility.Visible)
            {
                pbar.Visibility = Visibility.Hidden;
            }
            else
            {
                pbar.Visibility = Visibility.Visible;
            }
        }

        private void comboBoxUrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Url u = comboBoxUrl.SelectedValue as Url;
            if (u != null)
            {
                if (u.name.Equals("Custom"))
                {
                    txtCustomUrl.Text = string.Empty;
                    txtCustomUrl.Visibility = Visibility.Visible;
                }
                else
                {
                    txtCustomUrl.Text = u.url;
                    txtCustomUrl.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void newInstance_window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
