using Newtonsoft.Json;
using SalesforcePackager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace SalesforcePackager.Common
{
    public class Utility
    {
        public const double API_VERSION = 38.0;
        public const string APPLICATION_FOLDER = "Packager";
        public const string CACHE_NAME = "cache.json"; 

        public static void saveProjectData(Project project)
        {
            string json = JsonConvert.SerializeObject(project);
            CheckDir(project.projectPath);
            string projectDataPath = string.Format(@"{0}\{1}", project.projectPath, "project.json");
            File.WriteAllText(projectDataPath, json);
        }

        public static Project readProjectData(string projectPath)
        {
            if (File.Exists(projectPath))
            {
                string json = File.ReadAllText(projectPath);
                return JsonConvert.DeserializeObject<Project>(json);
            }
            else
            {
                return null;
            }
        }  
         
        /// <summary>
        /// Check the specified folder, and create if it doesn't exist.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static string CheckDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public static string AppPath
        {
            get
            {
                string folderBase = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string dir = string.Format(@"{0}\{1}", folderBase, APPLICATION_FOLDER);
                return CheckDir(dir);
            }
        }

        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }
    }
}
