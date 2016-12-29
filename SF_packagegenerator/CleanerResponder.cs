using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforcePackager
{
    interface CleanerResponder
    {
        void started(int max);
        void progress(int remaining, string currentProfile);
        void finished(string result, bool isSuccess);
    }
}
