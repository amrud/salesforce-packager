using SalesforcePackager.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforcePackager.Model
{
    public class Project
    {
        public string name { get; set; }
        public string dataPath
        {
            get
            {
                return string.Format(@"{0}\{1}", projectPath, "project.json");
            }
        }
        public string sfUrl { get; set; }
        public string sfInstance { get; set; }
        public string sfInstanceOrgId { get; set; }
        public string projectPath { get; set; }

        public List<FileProperties> components { get; set; } 
        public DateTime createdDate { get; set; }
    }
}
