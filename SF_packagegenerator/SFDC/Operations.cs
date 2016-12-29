using SalesforcePackager.Metadata;
using SalesforcePackager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SalesforcePackager.SFDC
{
    class Operations
    {
        private const int ONE_SECOND = 1000;
        private const int MAX_NUM_POLL_REQUEST = 50;
        private static bool loggedIn = false;

        public static DescribeMetadataResult getComponentList(Instance instance)
        {
            try
            {
                if (instance == null)
                {
                    ExceptionHandler x = new ExceptionHandler();
                    x.ErrorMessage = "No instance";
                    throw x;
                }

                LoginMessage loginMessage = null;
                if (!loggedIn)
                {
                    loginMessage = SFDC.Login.login(instance);
                }

                if (loginMessage != null && loginMessage.isLogin)
                {
                    MetadataService metadata = new MetadataService();
                    metadata.SessionHeaderValue = new Metadata.SessionHeader();
                    metadata.SessionHeaderValue.sessionId = loginMessage.loginResult.sessionId;
                    metadata.Url = loginMessage.loginResult.serverUrl.Replace(@"/u/", @"/m/");

                    DescribeMetadataResult dmr = metadata.describeMetadata(Common.Utility.API_VERSION);
                    return dmr;
                }
                else
                {
                    ExceptionHandler x = new ExceptionHandler();
                    x.ErrorMessage = loginMessage.message;
                    throw x;
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        public static List<FileProperties> getComponentItemsDetails(Instance instance, string type, string folderName = null)
        {
            try
            {
                LoginMessage loginMessage = null;
                if (!loggedIn)
                {
                    loginMessage = SFDC.Login.login(instance);
                }

                if (loginMessage != null && loginMessage.isLogin)
                {
                    MetadataService metadata = new MetadataService();
                    metadata.SessionHeaderValue = new Metadata.SessionHeader();
                    metadata.SessionHeaderValue.sessionId = loginMessage.loginResult.sessionId;
                    metadata.Url = loginMessage.loginResult.serverUrl.Replace(@"/u/", @"/m/");

                    ListMetadataQuery query = new ListMetadataQuery();
                    query.type = type;
                    query.folder = folderName;

                    List<ListMetadataQuery> listMetadataQuery = new List<ListMetadataQuery>();
                    listMetadataQuery.Add(query);
                    FileProperties[] files = metadata.listMetadata(listMetadataQuery.ToArray(), Common.Utility.API_VERSION);
                    return files != null ? files.ToList() : null;
                }
                else
                {
                    throw new ExceptionHandler() { ErrorMessage = loginMessage.message };
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void generatePackage(Instance instance, Project project)
        {
            try
            {
                LoginMessage loginMessage = null;
                if (!loggedIn)
                {
                    loginMessage = SFDC.Login.login(instance);
                }

                if (loginMessage != null && loginMessage.isLogin)
                {

                    List<PackageTypeMembers> types = new List<PackageTypeMembers>();

                    var results = from fp in project.components
                                  group fp.fullName by fp.type into c
                                  select new { name = c.Key, items = c.ToList() };

                    foreach (var component in results)
                    {
                        PackageTypeMembers packageTypeMember = new PackageTypeMembers();
                        packageTypeMember.name = returnedWorkingMetadataName(component.name);
                        packageTypeMember.members = component.items.ToArray();
                        types.Add(packageTypeMember);
                    }


                    Package packageManifest = new Package();
                    packageManifest.types = types.ToArray();

                    RetrieveRequest request = new RetrieveRequest();
                    request.apiVersion = Common.Utility.API_VERSION;
                    request.singlePackage = true;
                    request.unpackaged = packageManifest;

                    MetadataService metadata = new MetadataService();
                    metadata.SessionHeaderValue = new Metadata.SessionHeader();
                    metadata.SessionHeaderValue.sessionId = loginMessage.loginResult.sessionId;
                    metadata.Url = loginMessage.loginResult.serverUrl.Replace(@"/u/", @"/m/");
                    
                    AsyncResult result = metadata.retrieve(request);

                    string asyncResultId = result.id;
                    int poll = 0;
                    int waitTimeMilliSecs = ONE_SECOND;

                    RetrieveResult retrieveResult = null;

                    do
                    {
                        Thread.Sleep(waitTimeMilliSecs);
                        waitTimeMilliSecs *= 2;
                        if (poll++ > MAX_NUM_POLL_REQUEST)
                        {
                            throw new Exception("Request timed out.  If this is a large set " +
                        "of metadata components, check that the time allowed " +
                        "by MAX_NUM_POLL_REQUESTS is sufficient.");
                        }

                        retrieveResult = metadata.checkRetrieveStatus(asyncResultId, true);

                    } while (!retrieveResult.done);

                    if (retrieveResult.status == RetrieveStatus.Failed)
                    {
                        throw new Exception(retrieveResult.errorStatusCode + " msg: " + retrieveResult.errorMessage);
                    }
                    else if (retrieveResult.status == RetrieveStatus.Succeeded)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        if (retrieveResult.messages != null)
                        {
                            foreach (RetrieveMessage message in retrieveResult.messages)
                            {
                                stringBuilder.Append(message.fileName + "-" + message.problem);
                            }
                        }

                        if (stringBuilder.Length > 0)
                        {
                            // textBox.Text += "\nRetrieve warnings:" + stringBuilder.ToString();
                        }

                        if (retrieveResult.zipFile.Length == 0) return;

                        string zipPath = string.Format(@"{0}\{1}", project.projectPath, "retrieveResults.zip");
                        string extractPath = string.Format(@"{0}\{1}", project.projectPath, "Package");
                        using (FileStream resultsFile = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            resultsFile.Write(retrieveResult.zipFile, 0, retrieveResult.zipFile.Length);
                            resultsFile.Close();
                        }

                        //extract and remove the file

                        if (File.Exists(zipPath))
                        {
                            if (Directory.Exists(extractPath))
                            {
                                Common.Utility.DeleteDirectory(extractPath);
                            }

                            ZipFile.ExtractToDirectory(zipPath, extractPath);
                            File.Delete(zipPath);
                        }
                    }
                }
                else
                {
                    throw new ExceptionHandler() { ErrorMessage = loginMessage.message };
                }
            }
            catch (Exception)
            {
                throw;
            }

        }


        private static string returnedWorkingMetadataName(string component)
        {
            if (component.Equals("ReportFolder"))
            {
                return "Report";
            }
            else if (component.Equals("EmailFolder"))
            {
                return "EmailTemplate";
            }
            else if (component.Equals("DocumentFolder"))
            {
                return "Document";
            }
            else if (component.Equals("DashboardFolder"))
            {
                return "Dashboard";
            }

            return component;
        }

        public static SFDC.GetUserInfoResult getUserInfoSample(Instance instance)
        {
            LoginMessage loginMessage = null;
            if (!loggedIn)
            {
                loginMessage = SFDC.Login.login(instance);
            }

            if (loginMessage != null && loginMessage.isLogin)
            {
                try
                {
                    SFDC.SforceService binding;
                    /*
                     * Create the binding to the sforce servics
                     */
                    binding = new SFDC.SforceService();
                    binding.Url = loginMessage.loginResult.serverUrl;
                    binding.SessionHeaderValue = new SFDC.SessionHeader();
                    binding.SessionHeaderValue.sessionId = loginMessage.loginResult.sessionId;
                    SFDC.GetUserInfoResult userInfo = binding.getUserInfo();

                    return userInfo;
                }
                catch (Exception ex3)
                {
                    Console.WriteLine("ERROR: getting user info.\n" + ex3.Message);
                    
                }
            }

            return null;
        }
    }

}
