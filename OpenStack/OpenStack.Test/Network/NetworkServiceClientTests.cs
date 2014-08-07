// /* ============================================================================
// Copyright 2014 Hewlett Packard
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;
using OpenStack.Network;

namespace OpenStack.Test.Network
{
    [TestClass]
    public class NetworkServiceClientTests
    {
        internal TestNetworkServicePocoClient ServicePocoClient;
        
        internal string authId = "12345";
        internal string endpoint = "http://testcomputeendpoint.com/v2/1234567890";
        internal ServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.ServicePocoClient = new TestNetworkServicePocoClient();
            this.ServiceLocator = new ServiceLocator();
            
            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(INetworkServicePocoClientFactory), new TestNetworkServicePocoClientFactory(this.ServicePocoClient));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.ServicePocoClient = new TestNetworkServicePocoClient();
            this.ServiceLocator = new ServiceLocator();
        }

        IOpenStackCredential GetValidCreds()
        {
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Neutron", "Network Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(endpoint, string.Empty, "some version", "some version info", "1,2,3")
                }));

            var creds = new OpenStackCredential(new Uri(this.endpoint), "SomeUser", "Password", "SomeTenant");
            creds.SetAccessTokenId(this.authId);
            creds.SetServiceCatalog(catalog);
            return creds;
        }

        [TestMethod]
        public async Task CanGetFlavors()
        {
            var ntw1 = new OpenStack.Network.Network("12345","MyNetwork", NetworkStatus.Active);
            var ntw2 = new OpenStack.Network.Network("54321", "NetworkMy", NetworkStatus.Down);
            var networks = new List<OpenStack.Network.Network>() { ntw1, ntw2 };

            this.ServicePocoClient.GetNetworksDelegate = () => Task.Factory.StartNew(() => (IEnumerable<OpenStack.Network.Network>)networks);

            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetNetworks();
            Assert.IsNotNull(resp);

            var respNetworks = resp.ToList();
            Assert.AreEqual(2, respNetworks.Count());
            Assert.AreEqual(ntw1, respNetworks[0]);
            Assert.AreEqual(ntw2, respNetworks[1]);
        }
    }
}
