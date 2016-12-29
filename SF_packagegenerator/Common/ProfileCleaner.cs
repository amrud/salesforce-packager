using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SalesforcePackager.Common
{
    class ProfileCleaner
    {
        private String _profileFolderPath;
        public String[] profileFiles;
        private CleanerResponder cResponder;

        public ProfileCleaner(String projectPath, CleanerResponder resp)
        {
            try
            {
                _profileFolderPath = string.Format(@"{0}\{1}\{2}", projectPath, "Package", "profiles");
                profileFiles = Directory.GetFiles(_profileFolderPath, "*.profile");

                this.cResponder = resp;   
            }
            catch (Exception) { }
        }

        public void ReadProfileAndRemoveUserPermission()
        {
            string result ="";
            bool isSuccess = true;

            if (profileFiles != null)
            {
                int max= profileFiles.Count();
                cResponder.started(max);
                for (int index = 0; index < max; index++)
                {
                    var item = profileFiles[index];
                    try
                    {
                        string profileFileName = item.Substring(item.LastIndexOf("\\")+1).Replace(".profile","");
                        cResponder.progress(index, profileFileName);
                        XDocument xmlDoc = XDocument.Load(item, LoadOptions.None);
                        xmlDoc.Descendants().Where(e => e.Name.LocalName == "userPermissions").Remove();
                        xmlDoc.Save(item);
                    }
                    catch (Exception ex)
                    {
                        result = ex.Message;
                        isSuccess = false;
                        break;
                    }
                   
                }

            }
            else
            {
                result = "No Profile found";
                isSuccess = false;
            }

            cResponder.finished(result, isSuccess);                         
        }
            
    }
}
