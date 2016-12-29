using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforcePackager.Model
{
    class LoginMessage
    {
        public bool isLogin { get; set; }
        public string message { get; set; }
        public SFDC.LoginResult loginResult { get; set; }
    }
}
