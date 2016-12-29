using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SalesforcePackager.Common;
using SalesforcePackager.Metadata;
using SalesforcePackager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for DeploymentDetails.xaml
    /// </summary>
    public partial class DeploymentDetails : Page
    {
        private Project project;
        private List<FileProperties> originalList;
        public event EventHandler toastHandler;

        public DeploymentDetails(Project p)
        {
            InitializeComponent();

            this.project = p;
            if (project != null)
            {
                initProject();
            }
        }

        private void initProject(bool firstLoad = true)
        {
            if (firstLoad)
            {
                //lblName.Content += "\t\t: " + project.name;
                //lblInstance.Content += "\t\t: " + project.sfInstance;
                lblCreatedDate.Content += project.createdDate.ToString("dd MMMM yyyy");
                //lblOrgId.Content += "\t: " + Common.Settings.getInstanceSetting(project.sfInstance).instanceOrgId;
                btnAddComponent.IsEnabled = btnGeneratePackage.IsEnabled = Common.Settings.getInstanceSetting(project.sfInstanceOrgId) != null;
            }

            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = originalList = project.components.OrderBy(x => x.fullName).ToList();
            spCountTotal.Visibility = project.components.Count == 0 ? Visibility.Hidden : Visibility.Visible;

            if (!project.components.Any())
            {
                btnGeneratePackage.IsEnabled = false;
            }
            else
            {
                btnGeneratePackage.IsEnabled = true;

                List<String> availableTypes = new List<string>();
                foreach (var item in project.components)
                {
                    if (!availableTypes.Contains(item.type))
                    {
                        availableTypes.Add(item.type);
                    }
                }
                 
                spCount.Children.Clear();
                
                spCountTotal.Content = "Total Components " + project.components.Count; 
                foreach (var item in availableTypes)
                {
                    String str = item + " " + CountOccurenceOfValue(project.components, item);
                    Label lbl = new Label();
                    lbl.FontSize = 12.0;
                    lbl.Foreground = Brushes.White;
                    lbl.Content = str;
                    spCount.Children.Add(lbl);
                } 
            }

        }

        private int CountOccurenceOfValue(List<FileProperties> list, string valueToFind)
        {
           return ((from temp in list where temp.type.Equals(valueToFind) select temp).Count()); 
        }

        private void btnAddComponent_Click(object sender, RoutedEventArgs e)
        {
            AddComponent addCompWindow = new AddComponent(project);
            addCompWindow.Owner = Application.Current.MainWindow;
            addCompWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            addCompWindow.Top = Application.Current.MainWindow.Top + 30;
            addCompWindow.Left = Application.Current.MainWindow.Left;
            addCompWindow.Height = Application.Current.MainWindow.Height - 30;
            addCompWindow.Width = Application.Current.MainWindow.Width;
            addCompWindow.Closed += (s, evetArg) =>
            {
                project = Common.Utility.readProjectData(project.dataPath);
                initProject(false);
            };
            addCompWindow.ShowDialog();
        }

        private async void BtnRemove_Clicked(object sender, RoutedEventArgs e)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            MessageDialogResult confirmDeleteDialog = await metroWindow.ShowMessageAsync("Remove Confirmation", "Do you want to remove this component?", MessageDialogStyle.AffirmativeAndNegative);
            if (confirmDeleteDialog == MessageDialogResult.Affirmative)
            {
                Metadata.FileProperties fp = dataGrid.SelectedItem as Metadata.FileProperties;
                project.components.Remove(fp);
                Common.Utility.saveProjectData(project);
                initProject(false);
            }
            
        }

        private void btnGeneratePackage_Click(object sender, RoutedEventArgs e)
        {

            if (project.components.Any())
            {
                dataGrid.IsEnabled = false;
                pbarGenerate.Visibility = Visibility.Visible;
                btnAddComponent.IsEnabled = false;
                btnGeneratePackage.IsEnabled = false;
                BackgroundWorker backgroundGenerate = new BackgroundWorker();
                backgroundGenerate.DoWork += BackgroundGenerate_DoWork;
                backgroundGenerate.RunWorkerCompleted += BackgroundGenerate_RunWorkerCompleted;
                backgroundGenerate.RunWorkerAsync();
            }
        }

        private void BackgroundGenerate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pbarGenerate.Visibility = Visibility.Hidden;
            btnGeneratePackage.IsEnabled = true;
            btnAddComponent.IsEnabled = true;
            dataGrid.IsEnabled = true;
            if (toastHandler != null)
            {
                toastHandler(this, e);
            }
        }

        private void BackgroundGenerate_DoWork(object sender, DoWorkEventArgs e)
        {
            SFDC.Operations.generatePackage(Common.Settings.getInstanceSetting(project.sfInstanceOrgId), project);
        }

        private void txtBox_Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBox_Filter.Text))
            {
                dataGrid.ItemsSource = originalList;
            }
            else
            {
                if (originalList != null && originalList.Any())
                {
                    var _itemSourceList = new CollectionViewSource() { Source = originalList };
                    // ICollectionView the View/UI part 
                    ICollectionView Itemlist = _itemSourceList.View;
                    // your Filter
                    var customFilter = new Predicate<object>(item => CaseInsensitiveContains(((FileProperties)item).fullName, txtBox_Filter.Text, StringComparison.OrdinalIgnoreCase)); //((FileProperties)item).fullName.Contains(txtBox_Filter.Text));
                                                                                                                                                                                        //now we add our Filter
                    Itemlist.Filter = customFilter;
                    dataGrid.ItemsSource = Itemlist;
                }
            }
        }

        private bool CaseInsensitiveContains(string text, string value,
            StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }

        private void dataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                deleteSelectedItems();
                e.Handled = true;
            }
        }

        private async void deleteSelectedItems()
        {
            List<FileProperties> selectedItems = dataGrid.SelectedItems.Cast<FileProperties>().ToList();
            if (selectedItems.Any())
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                MessageDialogResult confirmDeleteDialog = await metroWindow.ShowMessageAsync("Remove Confirmation", "Do you want to remove this components?", MessageDialogStyle.AffirmativeAndNegative);

                if (confirmDeleteDialog == MessageDialogResult.Affirmative)
                {

                    foreach (var item in selectedItems)
                    {
                        project.components.Remove(item);
                    }
                    Common.Utility.saveProjectData(project);
                    initProject(false);
                }

            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            deleteSelectedItems();
        }
         
        private void spCountTotal_MouseEnter(object sender, MouseEventArgs e)
        {
            componentCountPopup.IsOpen = true;
        }

        private void spCountTotal_MouseLeave(object sender, MouseEventArgs e)
        {
            componentCountPopup.IsOpen = false;
        }

        private void btnListProfiles_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
