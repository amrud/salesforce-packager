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
using System.Collections.ObjectModel;

namespace SalesforcePackager
{
    /// <summary>
    /// Interaction logic for TabControl.xaml
    /// </summary>
    public partial class TabControl : MetroWindow
    {
        public TabControl()
        {
            InitializeComponent();

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

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}
