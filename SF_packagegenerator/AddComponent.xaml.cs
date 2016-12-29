using MahApps.Metro.Controls;
using SalesforcePackager.Metadata;
using SalesforcePackager.Model;
using SalesforcePackager.SFDC;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SalesforcePackager
{
    /// <summary>
    /// Interaction logic for AddComponent.xaml
    /// </summary>
    public partial class AddComponent : MetroWindow
    {
        private Instance instance;
        private List<FileProperties> selectedFiles;
        private List<FileProperties> originalList;
        private Project project;
        private BackgroundWorker compDetailsWorker;
        public List<FileProperties> selectedItems;

        public AddComponent(Project p)
        {
            InitializeComponent(); 
           // this.SourceInitialized += AddComponent_SourceInitialized;

            this.project = p;
            initItems(); 
        }

        private void AddComponent_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
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

        private void initItems()
        {
            instance = Common.Settings.getInstanceSetting(project.sfInstanceOrgId);
            selectedFiles = project.components.Any() ? project.components : new List<FileProperties>();

            comboBox.IsEnabled = false;
            BackgroundWorker compListWorker = new BackgroundWorker();
            toggleProgress();
            compListWorker.DoWork += CompListWorker_DoWork;
            compListWorker.RunWorkerCompleted += CompListWorker_RunWorkerCompleted;
            compListWorker.RunWorkerAsync();
        }

        private void CompListWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result is DescribeMetadataResult)
                {
                    DescribeMetadataResult describeMetadataResultCache = e.Result as DescribeMetadataResult;

                    if (describeMetadataResultCache != null)
                    {
                        List<MetadataObject> metadataObjects = new List<MetadataObject>();
                        foreach (DescribeMetadataObject item in describeMetadataResultCache.metadataObjects)
                        {
                            MetadataObject mo = new MetadataObject()
                            {
                                xmlName = checkComponentName(item.xmlName),
                                inFolder = item.inFolder,
                                childXmlNames = item.childXmlNames,
                                directoryName = item.directoryName,
                                metaFile = item.metaFile,
                                suffix = item.suffix
                            };

                            if (notOptOutComponent(mo))
                            {
                                metadataObjects.Add(mo);
                            }

                        }
                        metadataObjects.Add(new MetadataObject() { xmlName = "CustomField", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "SharingTerritoryRule", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "SharingOwnerRule", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "SharingCriteriaRule", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "WorkflowRule", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "WorkflowAlert", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "WorkflowFieldUpdate", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "WorkflowOutboundMessage", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "WorkflowTask", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "ValidationRule", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "DocumentFolder", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "EmailFolder", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "ReportFolder", directoryName = "", suffix = "" });
                        metadataObjects.Add(new MetadataObject() { xmlName = "DashboardFolder", directoryName = "", suffix = "" });

                        comboBox.ItemsSource = from x in metadataObjects
                                               orderby x.xmlName
                                               select x;
                    }
                }
                else
                {
                    //show error
                    var error = e.Result as ExceptionHandler;
                }

            }
            catch (Exception)
            {

            }

            toggleProgress();
            comboBox.IsEnabled = true;
        }

        private bool notOptOutComponent(MetadataObject mo)
        {
            bool isNotOptOut = true;
            if (mo.xmlName.Equals("Flow"))
            {
                isNotOptOut = false;
            }
            else if (mo.xmlName.Equals("FlowDefinition"))
            {
                isNotOptOut = false;
            }

            return isNotOptOut;
        }

        private string checkComponentName(string xmlName)
        {
            string s;

            if (!string.IsNullOrEmpty(xmlName) && xmlName.Equals("CustomLabels"))
            {
                s = "CustomLabel";
            }
            else
            {
                s = xmlName;
            }

            return s;
        }

        private void CompListWorker_DoWork(object sender, DoWorkEventArgs e)
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
                    Common.Settings.saveInstanceSetting(instance, instance);
                }

                e.Result = describeMetadataResultCache;
            }
            catch (Exception ex)
            {
                ExceptionHandler handler = (ExceptionHandler)ex;
                e.Result = handler;
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                stackFolderName.Visibility = Visibility.Collapsed;

                if (compDetailsWorker != null && compDetailsWorker.IsBusy)
                {
                    compDetailsWorker.CancelAsync();
                }

                MetadataObject dmo = comboBox.SelectedItem as MetadataObject;

                if (dmo != null)
                {
                    if (dmo.xmlName.Equals("Report") || dmo.xmlName.Equals("EmailTemplate") || dmo.xmlName.Equals("Document") || dmo.xmlName.Equals("Dashboard"))
                    {
                        if (dmo.xmlName.Equals("Report"))
                        {
                            comboBoxFolder.ItemsSource = from x in instance.reportFolderCache
                                                         orderby x.fullName
                                                         select x;
                        }
                        else if (dmo.xmlName.Equals("EmailTemplate"))
                        {
                            comboBoxFolder.ItemsSource = from x in instance.emailFolderCache
                                                         orderby x.fullName
                                                         select x;
                        }
                        else if (dmo.xmlName.Equals("Document"))
                        {
                            comboBoxFolder.ItemsSource = from x in instance.documentFolderCache
                                                         orderby x.fullName
                                                         select x;
                        }
                        else if (dmo.xmlName.Equals("Dashboard"))
                        {
                            comboBoxFolder.ItemsSource = from x in instance.dashboardFolderCache
                                                         orderby x.fullName
                                                         select x;
                        }
                        stackFolderName.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        comboBoxFolder.ItemsSource = null;
                        dataGrid.ItemsSource = null;
                        toggleProgress();

                        compDetailsWorker = new BackgroundWorker();
                        compDetailsWorker.WorkerSupportsCancellation = true;
                        compDetailsWorker.DoWork += CompDetailsWorker_DoWork;
                        compDetailsWorker.RunWorkerCompleted += CompDetailsWorker_RunWorkerCompleted;
                        compDetailsWorker.RunWorkerAsync(dmo);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void CompDetailsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                List<FileProperties> fileProperties = e.Result as List<FileProperties>;
                if (fileProperties != null)
                {
                    if (fileProperties.Any())
                    {
                        lblEmpty.Visibility = Visibility.Collapsed;
                        List<string> fileExists = project.components != null ? project.components.Select(y => y.id + '_' + y.fullName + '_' + y.type).ToList() : new List<string>();
                        dataGrid.ItemsSource = originalList = fileProperties.Where(x => !fileExists.Contains(x.id + '_' + x.fullName + '_' + x.type)).OrderBy(x => x.fullName).ToList();
                    }
                    else
                    {
                        lblEmpty.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    lblEmpty.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {
                lblEmpty.Visibility = Visibility.Visible;
            }

            toggleProgress();
        }

        private void CompDetailsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                MetadataObject dmo = e.Argument as MetadataObject;

                List<FileProperties> fileProperties = SFDC.Operations.getComponentItemsDetails(instance, dmo.xmlName, dmo.folder);
                e.Result = fileProperties;
            }
            catch (Exception ex)
            {
                ExceptionHandler handler = (ExceptionHandler)ex;
                e.Result = handler;
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            project.components = selectedFiles;
            List<FileProperties> selectedItems = dataGrid.SelectedItems.Cast<FileProperties>().ToList();
            if (selectedItems.Any())
            {
                selectedFiles.RemoveAll(x => selectedItems.Contains(x));
                project.components.AddRange(selectedItems);
            }
            Common.Utility.saveProjectData(project);
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FileProperties fp = dataGrid.SelectedItem as FileProperties;
            if (!selectedFiles.Contains(fp))
            {
                selectedFiles.Add(fp);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FileProperties fp = dataGrid.SelectedItem as FileProperties;
            selectedFiles.Remove(fp);
        }

        private void btnCheckall_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);

                if (row != null)
                {
                    CheckBox chk = dataGrid.Columns[0].GetCellContent(row) as CheckBox;
                    chk.IsChecked = true;
                }
            }
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
                    var customFilter = new Predicate<object>(item => CaseInsensitiveContains(((FileProperties)item).fullName, txtBox_Filter.Text, StringComparison.OrdinalIgnoreCase)); //now we add our Filter
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

        private void comboBoxFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                if (compDetailsWorker != null && compDetailsWorker.IsBusy)
                {
                    compDetailsWorker.CancelAsync();
                }

                FileProperties fp = comboBoxFolder.SelectedItem as FileProperties;

                if (fp != null)
                {
                    String type = fp.type.Equals("ReportFolder") ? "Report" : fp.type.Equals("EmailFolder") ? "EmailTemplate" : fp.type.Equals("DocumentFolder") ? "Document" : "Dashboard";
                    MetadataObject dmo = new MetadataObject();
                    dmo.xmlName = type;
                    dmo.folder = fp.fullName;

                    dataGrid.ItemsSource = null;
                    toggleProgress();

                    compDetailsWorker = new BackgroundWorker();
                    compDetailsWorker.WorkerSupportsCancellation = true;
                    compDetailsWorker.DoWork += CompDetailsWorker_DoWork;
                    compDetailsWorker.RunWorkerCompleted += CompDetailsWorker_RunWorkerCompleted;
                    compDetailsWorker.RunWorkerAsync(dmo);

                }
            }
            catch (Exception)
            {

            }
        }
        
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblSelectedItem.Content = dataGrid.SelectedItems.Count > 0 ? dataGrid.SelectedItems.Count + " items selected" : string.Empty;
        } 

        private void addComp_Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
