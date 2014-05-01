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
using System.Threading.Tasks;
using OpenStack;
using OpenStack.Identity;

namespace CustomServiceClientExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var authUri = new Uri("https://region.identity.host.com:12345/v2.0/tokens");
            var userName = "user name";
            var password = "password";
            var tenantId = "XXXXXXXXXXXXXX-Project";
            var echoMessage = "Hello world!";

            Console.WriteLine("Calling remote service to echo the following message: '{0}'", echoMessage);

            var echoMessageTask = EchoMessage(echoMessage, authUri, userName, password, tenantId);
            echoMessageTask.Wait();

            Console.WriteLine("Response from remote service: '{0}'", echoMessageTask.Result);
            Console.ReadLine();
        }

        public static async Task<string> EchoMessage(string message, Uri authUri, string userName, string password, string tenantId)
        {
            var credential = new OpenStackCredential(authUri, userName, password, tenantId);
            var client = OpenStackClientFactory.CreateClient(credential);
            
            await client.Connect();

            var echoServiceClient = client.CreateServiceClient<EchoServiceClient>();
            var resp = await echoServiceClient.Echo(message);
            return resp.Message;
        }
    }
}