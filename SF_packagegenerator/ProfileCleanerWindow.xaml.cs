using Ookii.Dialogs.Wpf;
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
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using SalesforcePackager.Common;
using System.ComponentModel;
using System.Diagnostics;

namespace SalesforcePackager
{
    /// <summary>
    /// Interaction logic for ProfileCleaner.xaml
    /// </summary>
    public partial class ProfileCleanerWindow : MetroWindow, CleanerResponder
    {
        private BackgroundWorker cleanupProfile;
        private string _result;
        private bool _isSuccess;

        public ProfileCleanerWindow()
        {
            InitializeComponent();
             
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
                        txtBoxFolderPath.Text = path;
                    }
                    else
                    {
                        throw new Exception();
                    }

                }
                catch (Exception)
                { 
                    await this.ShowMessageAsync("Invalid Directory", "Please select a valid directory");
                }

            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (btnSubmit.Content.Equals("submit"))
            {

                string path = txtBoxFolderPath.Text;
                if (!string.IsNullOrEmpty(path))
                {

                    toggleProgress();
                    txtBoxStatus.Text = "Starting profile cleaner";
                    cleanupProfile = new BackgroundWorker();
                    cleanupProfile.DoWork += CleanupProfile_DoWork;
                    cleanupProfile.ProgressChanged += CleanupProfile_ProgressChanged;
                    cleanupProfile.RunWorkerCompleted += CleanupProfile_RunWorkerCompleted;
                    cleanupProfile.WorkerReportsProgress = true;
                    cleanupProfile.RunWorkerAsync(path);
                }
            }
            else if (btnSubmit.Content.Equals("Open"))
            {
                string path = txtBoxFolderPath.Text;
                try
                {
                    // opens the folder in explorer
                    Process.Start(path);
                }
                catch (Exception)
                {
                    // opens the folder in explorer
                    Process.Start(@"c:\");
                }
            }
        }

        private void CleanupProfile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string remaining = (string)e.UserState;
            if (remaining.Equals("starting"))
            {
                pbar.Maximum = e.ProgressPercentage;
            }
            else
            {
                pbar.Value = e.ProgressPercentage;
                txtBoxStatus.Text += "\n" + remaining;
            }
        }

        private void CleanupProfile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toggleProgress();
            txtBoxStatus.Text += "\nProfiles updated";
            if (_isSuccess)
            {
                btnSubmit.Content = "Open";
            }
            else
            {

            }
        }

        private void CleanupProfile_DoWork(object sender, DoWorkEventArgs e)
        {
            ProfileCleaner profileCleaner = new ProfileCleaner(e.Argument as string, this);
            profileCleaner.ReadProfileAndRemoveUserPermission();
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

        public void started(int max)
        {
            cleanupProfile.ReportProgress(max, "starting");
        }

        public void progress(int remaining, string item)
        {
            cleanupProfile.ReportProgress(remaining, item);           
        }

        public void finished(string result, bool isSuccess)
        {
            _isSuccess = isSuccess;
            _result = result;
        }
    }
}
