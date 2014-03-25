using System;
using System.Linq;
using System.Security;
using System.Threading;
using Openstack;
using Openstack.Identity;
using Openstack.Storage;

namespace SimpleStorageExample
{
    class Program
    {
        static void Main(string[] args)
        {
            //enter your user name, password, tenant Id, and the authorization endpoint
            //for the instance of openstack that you want to connect to.
            var authUri = new Uri("https://region.identity.host.com:12345/v2.0/tokens");
            var userName = "user name";
            var password = "password";
            var tenantId = "tenant Id"; // e.g. XXXXXXXXXXXXX-Project

            //Convert the plain text password into a SecureString. 
            //Ideally you should never store your passwords in plain text, but for this example we will.
            var securePassword = new SecureString();
            password.ToCharArray().ToList().ForEach(securePassword.AppendChar);

            //Construct an OpenstackCredential object that will be used to authenticate.
            //The credential will also be useful later as it contains a reference to the service catalog, and access token.
            var credential = new OpenstackCredential(authUri, userName, securePassword, tenantId);

            //Create a new OpenStackClient object using the credentials you just created.
            //A cancellation token can also be supplied, this will allow you to cancel any long running tasks
            //that the client may make.
            var client = new OpenstackClient(credential, CancellationToken.None);

            //Connect the client to Openstack. This will authenticate you, as well as construct the service catalog, 
            //and retrieve the access token that will be used in future calls to Openstack services.
            var connectTask = client.Connect();

            //Console applications can't do async, so we need to wait on the task, 
            //in other contexts you can use the wait keyword.
            connectTask.Wait();  

            //Once the OpenstackClient has been connected, you can request a service client from it.
            //The service client will be created with the credentials that you have already specified, 
            //and do not need any additional information for you to interact with them.
            var storageClient = client.CreateServiceClient<IStorageServiceClient>();

            //Once we have the storage service client, we can ask it for the details of the current storage account.
            var getAccountTask = storageClient.GetStorageAccount();
            getAccountTask.Wait();
            var account = getAccountTask.Result;

            //Here we will write out the name of the account, and print out the names of each storage container in the account.
            Console.WriteLine("Connected to storage account '{0}'", account.Name);
            Console.WriteLine("Storage account '{0}' has the following containers:", account.Name);
            foreach (var container in account.Containers)
            {
                Console.WriteLine("\t{0}",container.Name);
            }
            Console.WriteLine(string.Empty);
            Console.ReadLine();
        }
    }
}
