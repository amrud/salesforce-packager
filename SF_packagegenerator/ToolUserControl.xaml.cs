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
    /// Interaction logic for ToolUserControl.xaml
    /// </summary>
    public partial class ToolUserControl : UserControl
    {
        public ToolUserControl()
        {
            InitializeComponent();
        }

        private void btnListProfiles_Click(object sender, RoutedEventArgs e)
        {
            ProfileCleanerWindow pcleaner = new ProfileCleanerWindow();
            pcleaner.ShowDialog();
        }
    }
}
