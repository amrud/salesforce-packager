using Squirrel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SF_packagegenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!string.IsNullOrEmpty(SalesforcePackager.Common.Settings.getAppCache().updaterPath))
            {
                using (var mgr = new UpdateManager(SalesforcePackager.Common.Settings.getAppCache().updaterPath))
                {
                    await mgr.UpdateApp();
                }
            }
        }
    }
}
