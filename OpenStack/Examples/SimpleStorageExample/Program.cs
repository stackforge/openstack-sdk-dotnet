// /* ============================================================================
// Copyright 2014 Hewlett Packard
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

using System;
using OpenStack;
using OpenStack.Identity;
using OpenStack.Storage;

namespace SimpleStorageExample
{
    class Program
    {
        static void Main(string[] args)
        {
            //enter your user name, password, tenant Id, and the authorization endpoint
            //for the instance of OpenStack that you want to connect to.
            var authUri = new Uri("https://region.identity.host.com:12345/v2.0/tokens");
            var userName = "user name";
            var password = "password";
            var tenantId = "tenant Id"; // e.g. XXXXXXXXXXXXX-Project

            //Construct an OpenStackCredential object that will be used to authenticate.
            //The credential will also be useful later as it contains a reference to the service catalog, and access token.
            var credential = new OpenStackCredential(authUri, userName, password, tenantId);

            //Create a new OpenStackClient object using the credentials you just created.
            var client = OpenStackClientFactory.CreateClient(credential);

            //Connect the client to OpenStack. This will authenticate you, as well as construct the service catalog, 
            //and retrieve the access token that will be used in future calls to OpenStack services.
            var connectTask = client.Connect();

            //Console applications can't do async, so we need to wait on the task, 
            //in other contexts you can use the wait keyword.
            connectTask.Wait();  

            //Once the OpenStackClient has been connected, you can request a service client from it.
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
