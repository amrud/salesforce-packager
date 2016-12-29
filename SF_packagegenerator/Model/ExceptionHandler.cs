using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforcePackager.Model
{
    class ExceptionHandler : Exception
    {
        private string errorMessage;
        public string ErrorMessage {
            get
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    return Message;
                }
                else
                {
                    return errorMessage;
                }
            }
            set
            {
                errorMessage = value;
            }
        }
        
    }
}
