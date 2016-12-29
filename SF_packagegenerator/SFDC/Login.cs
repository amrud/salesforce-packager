using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SalesforcePackager.Model;

namespace SalesforcePackager.SFDC
{
    class Login
    {

        /*
        private const String MANIFEST_FILE = "package.xml";
        private const int ONE_SECOND = 1000;
        private const int MAX_NUM_POLL_REQUEST = 50;
        private const double API_VERSION = 37.0;
        private SFDC.SforceService binding = null;
        private SFDC.LoginResult loginResult = null;
       
        private DateTime serverTime;

        private Metadata.MetadataService metadata;*/ 
        public const string ENDPOINT = "services/Soap/u/38.0";

        // Time out after a minute
        private const int TIMEOUT = 6000;

        public static LoginMessage login(Instance instance)
        {
            SFDC.SforceService binding;
            LoginMessage loginMessage = new LoginMessage();

            //textBox.Text += "\nCreating the binding to the web service...";
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            /*
             * Create the binding to the sforce servics
             */
            binding = new SFDC.SforceService();
            binding.Url = string.Format(@"{0}/{1}", instance.instanceUrl, ENDPOINT);

            binding.Timeout = TIMEOUT;

            //Attempt the login giving the user feedback
            //textBox.Text += "\nLOGGING IN NOW....";
            //binding.Proxy = new System.Net.WebProxy("localhost:8082");

            try
            {
                SFDC.LoginResult loginResult = binding.login(instance.instanceUsername, string.Concat(instance.instancePassword, instance.instanceToken));
               
                loginMessage.loginResult = loginResult;
                loginMessage.isLogin = true;
                loginMessage.message = "verified";
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                // This is likley to be caused by bad username or password
                loginMessage.isLogin = false;
                loginMessage.message = e.Message;
            }
            catch (Exception e)
            {
                // This is something else, probably comminication
                loginMessage.isLogin = false;
                loginMessage.message = e.Message;
            }

            return loginMessage;
        }

       
    }




    /*
     * 
     * 
     * 
     * 
     * 
     * 

         private bool login()
        {
            //textBox.Text += "\nCreating the binding to the web service...";
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

    binding = new SFDC.SforceService();
            if (isSandbox)
            {
                binding.Url = "https://test.salesforce.com/services/Soap/u/37.0";
            }

// Time out after a minute
binding.Timeout = 60000;

            //Attempt the login giving the user feedback
            //textBox.Text += "\nLOGGING IN NOW....";
            //binding.Proxy = new System.Net.WebProxy("localhost:8082");

            try
            {

                loginResult = binding.login(userName, String.Concat(password, secToken));

            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                // This is likley to be caused by bad username or password
             //   textBox.Text += e.Message + ", please try again.\n\nHit return to continue...";
                //Console.Write(e.Message + ", please try again.\n\nHit return to continue...");
                //Console.ReadLine();
                return false;
            }
            catch (Exception)
            {
                // This is something else, probably comminication
             //   textBox.Text += (e.Message + ", please try again.\n\nHit return to continue...");
                //Console.Write(e.Message + ", please try again.\n\nHit return to continue...");
                //Console.ReadLine();
                return false;
            }

            //textBox.Text += "\nThe session id is: " + loginResult.sessionId;
            //Console.WriteLine("\nThe session id is: " + loginResult.sessionId);
           // textBox.Text += "\nThe new server url is: " + loginResult.serverUrl;
            //Console.WriteLine("\nThe new server url is: " + loginResult.serverUrl);


            //Change the binding to the new endpoint
            binding.Url = loginResult.serverUrl;

            //Create a new session header object and set the session id to that returned by the login
            binding.SessionHeaderValue = new SFDC.SessionHeader();
            binding.SessionHeaderValue.sessionId = loginResult.sessionId;

            loggedIn = true;

            // call the getServerTimestamp method
            getServerTimestampSample();

            ///call the getUserInfo method
            getUserInfoSample();

            return true;
        }


        private void getServerTimestampSample()
{
    //Verify that we are already authenticated, if not
    //call the login function to do so
    if (!loggedIn)
    {
        if (!login())
            return;
    }

    try
    {
        //  textBox.Text += ("\nGetting server's timestamp...");
        //call the getServerTimestamp method
        SFDC.GetServerTimestampResult gstr = binding.getServerTimestamp();
        serverTime = gstr.timestamp;
        //access the return properties
        // textBox.Text += (gstr.timestamp.ToLongDateString() + " " + gstr.timestamp.ToLongTimeString());
    }
    catch (Exception ex2)
    {
        // textBox.Text += ("ERROR: getting server timestamp.\n" + ex2.Message);
    }

}

private void getUserInfoSample()
{
    //Verify that we are already authenticated, if not
    //call the login function to do so
    if (!loggedIn)
    {
        if (!login())
            return;
    }

    try
    {
        // textBox.Text += ("\nGetting user info...");
        //call the getUserInfo method
        userInfo = binding.getUserInfo();
        //access the return properties
        // textBox.Text += ("\nUser Name: " + userInfo.userFullName);
        // textBox.Text += ("\nUser Email: " + userInfo.userEmail);
        //  textBox.Text += ("\nUser Language: " + userInfo.userLanguage);
        //  textBox.Text += ("\nUser Organization: " + userInfo.organizationName);

        /*Console.WriteLine("\nGetting user info...");
        //call the getUserInfo method
        userInfo = binding.getUserInfo();
        //access the return properties
        Console.WriteLine("User Name: " + userInfo.userFullName);
        Console.WriteLine("User Email: " + userInfo.userEmail);
        Console.WriteLine("User Language: " + userInfo.userLanguage);
        Console.WriteLine("User Organization: " + userInfo.organizationName);
    }
    catch (Exception ex3)
    {
        Console.WriteLine("ERROR: getting user info.\n" + ex3.Message);
    }
}
private void retrieveFromJSON()
        {
            if (!loggedIn)
            {

                if (!login())
                {
                    return;
                }

            }

           // textBox.Text += "\n\nRetrieve From JSON";

            string json = File.ReadAllText("cache.txt");

            List<SFDCComponent> components = JsonConvert.DeserializeObject<List<SFDCComponent>>(json);

            List<PackageTypeMembers> types = new List<PackageTypeMembers>();

            foreach (SFDCComponent component in components)
            {
                PackageTypeMembers packageTypeMember = new PackageTypeMembers();
                packageTypeMember.name = component.name;
                packageTypeMember.members = component.items.ToArray();
                types.Add(packageTypeMember);
            }


            Package packageManifest = new Package();
            packageManifest.types = types.ToArray();

            RetrieveRequest request = new RetrieveRequest();
            request.apiVersion = API_VERSION;
            request.singlePackage = true;
            request.unpackaged = packageManifest;

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

                try
                {
                    if (retrieveResult.zipFile.Length == 0) return;
                    using (FileStream resultsFile = new FileStream("retrieveResults.zip", FileMode.Create, FileAccess.ReadWrite))
                    {
                        resultsFile.Write(retrieveResult.zipFile, 0, retrieveResult.zipFile.Length);
                        resultsFile.Close();
                    }
                }
                catch (Exception exception)
                {

                    throw exception;
                }

            }
        }



        */
}
