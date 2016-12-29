using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforcePackager.Model
{
    public class ApplicationData
    {
        public string version { get; set; }
        public SalesforcePackager.Common.Settings.Theme theme { get; set; }
        public List<Project> recentProjects { get; set; }
        public List<Instance> instanceList { get; set; }
        public string defaultProjectPath { get; set; }
        public string updaterPath { get; set; }
    }
}
