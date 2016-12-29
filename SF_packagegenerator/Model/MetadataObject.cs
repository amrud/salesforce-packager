using SalesforcePackager.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforcePackager.Model
{
    class MetadataObject : DescribeMetadataObject
    {
 
        public string name
        {
            get
            {
                var s = xmlName;
                s = string.Join(
                        string.Empty,
                        s.Select((x, i) => (
                             char.IsUpper(x) && i > 0 &&
                             (char.IsLower(s[i - 1]) || (i < s.Count() - 1 && char.IsLower(s[i + 1])))
                        ) ? " " + x : x.ToString()));

                return s;
            }
        }

        public string folder { get; set; }
    }
}
