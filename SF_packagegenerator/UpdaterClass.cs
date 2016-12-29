using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squirrel;

namespace SalesforcePackager
{
    class UpdaterClass
    {
        public static async Task updateApp()
        {
            using (var mgr = new UpdateManager(Common.Settings.getAppCache().updaterPath))
            {
                ReleaseEntry re =  await mgr.UpdateApp();
                
            }
        }
    }
}
