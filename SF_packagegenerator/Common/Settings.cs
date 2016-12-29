using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesforcePackager.Model;
using System.Collections;
using Newtonsoft.Json;
using System.Reflection;
using System.Configuration;
using System.IO;

namespace SalesforcePackager.Common
{
    public class Settings
    {
        private static String APPCACHE = "../AppCache.mdr";

        public enum Theme
        {
            LIGHT,
            DARK
        }

        public static Instance getInstanceSetting(string orgId)
        {
            Instance instance = null;
            List<Instance> instances = getInstanceSettings();

            foreach (Instance i in instances)
            {
                if (i.instanceOrgId.Equals(orgId))
                {
                    instance = i;
                    break;
                }
            }

            return instance;
        }

        public static void addInstanceSetting(Instance instance)
        {
            ApplicationData appData = getAppCache();
            List<Instance> instanceList = appData.instanceList;

            if (!instanceList.Any())
            {
                instanceList = new List<Instance>();
            }

            instanceList.Add(instance);
            appData.instanceList = instanceList;
            updateAppCache(appData);
        }
        
        public static void saveInstanceSetting(Instance oldInstance, Instance newInstance)
        {
            ApplicationData appData = getAppCache();
            List<Instance> instanceList = appData.instanceList; 
            foreach (var item in instanceList.Where(x => x.instanceName == oldInstance.instanceName))
            {
                item.instanceName = newInstance.instanceName;
                item.instancePassword = newInstance.instancePassword;
                item.instanceToken = newInstance.instanceToken;
                item.instanceUrl = newInstance.instanceUrl;
                item.instanceUsername = newInstance.instanceUsername;
                item.emailFolderCache = newInstance.emailFolderCache;
                item.reportFolderCache = newInstance.reportFolderCache;
                item.documentFolderCache = newInstance.documentFolderCache;
                item.dashboardFolderCache = newInstance.dashboardFolderCache;
                item.describeMetadataResultCache = newInstance.describeMetadataResultCache;
                item.instanceOrgId = newInstance.instanceOrgId;
                item.instanceOrgName = newInstance.instanceOrgName;
            }

            appData.instanceList = instanceList;
            updateAppCache(appData);
        }

        public static List<Instance> getInstanceSettings()
        {
            ApplicationData appData = getAppCache();
            List<Instance> instanceList = appData.instanceList; 
            return instanceList;
        }

        #region AppCache

        public static void updateAppCache(ApplicationData app)
        {
            string json = JsonConvert.SerializeObject(app);              
            File.WriteAllText(APPCACHE, json);
        }      

        public static ApplicationData getAppCache()
        {
            string json = null;
            if (File.Exists(APPCACHE))
            {
                json = File.ReadAllText(APPCACHE);
            }
                          
            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<ApplicationData>(json);
            }
            else
            {
                ApplicationData appData = initializeApplication();
                updateAppCache(appData);
                return appData;
            }
        }

        private static ApplicationData initializeApplication()
        {
            ApplicationData appData = new ApplicationData();
            appData.theme = Theme.LIGHT;
            string version;
            try
            {
                //// get deployment version
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch (Exception)
            {
                //// you cannot read publish version when app isn't installed 
                //// (e.g. during debug)
                version = "development";
            }

            appData.defaultProjectPath = Common.Utility.AppPath;
            appData.version = version;
            appData.recentProjects = new List<Project>();
            appData.instanceList = new List<Instance>();
            return appData;
        }
        #endregion
    }
}
