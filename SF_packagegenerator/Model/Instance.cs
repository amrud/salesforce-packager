using SalesforcePackager.Metadata;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforcePackager.Model
{
    public class Instance
    {
        public string instanceName { get; set; }
        public string instanceUrl { get; set; }
        public string instanceUsername { get; set; }
        public string instancePassword { get; set; }
        public string instanceToken{ get; set; }
        public string instanceOrgId { get; set; }
        public string instanceOrgName { get; set; }
        public DescribeMetadataResult describeMetadataResultCache { get; set; }
        public List<FileProperties> emailFolderCache { get; set; }
        public List<FileProperties> reportFolderCache { get; set; }
        public List<FileProperties> documentFolderCache { get; set; }
        public List<FileProperties> dashboardFolderCache { get; set; }

        public Instance()
        {

        }
    }
}
