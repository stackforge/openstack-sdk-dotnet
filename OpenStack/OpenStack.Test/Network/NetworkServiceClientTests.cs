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
        public async Task CanGetNetworks()
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

        [TestMethod]
        public async Task CanGetFloatingIps()
        {
            var ip1 = new OpenStack.Network.FloatingIp("12345", "172.0.0.1", FloatingIpStatus.Active);
            var ip2 = new OpenStack.Network.FloatingIp("54321", "172.0.0.2", FloatingIpStatus.Down);
            var ips = new List<OpenStack.Network.FloatingIp>() { ip1, ip2 };

            this.ServicePocoClient.GetFloatingIpsDelegate = () => Task.Factory.StartNew(() => (IEnumerable<OpenStack.Network.FloatingIp>)ips);

            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetFloatingIps();
            Assert.IsNotNull(resp);

            var respIps = resp.ToList();
            Assert.AreEqual(2, respIps.Count());
            Assert.AreEqual(ip1, respIps[0]);
            Assert.AreEqual(ip2, respIps[1]);
        }

        [TestMethod]
        public async Task CanGetFloatingIp()
        {
            var ip1 = new OpenStack.Network.FloatingIp("12345", "172.0.0.1", FloatingIpStatus.Active);

            this.ServicePocoClient.GetFloatingIpDelegate = (ip) => Task.Factory.StartNew(() => ip1);

            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetFloatingIp("12345");
            Assert.IsNotNull(resp);
            Assert.AreEqual(ip1, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetFloatingIpWithNullFloatingIpIdThrows()
        {
            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            await client.GetFloatingIp(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetFloatingIpWithEmptyFloatingIpIdThrows()
        {
            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            await client.GetFloatingIp(string.Empty);
        }

        [TestMethod]
        public async Task CanCreateFloatingIp()
        {
            var ip1 = new OpenStack.Network.FloatingIp("12345", "172.0.0.1", FloatingIpStatus.Active);

            this.ServicePocoClient.CreateFloatingIpDelegate = (ip) => Task.Factory.StartNew(() => ip1);

            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            var resp = await client.CreateFloatingIp("12345");
            Assert.IsNotNull(resp);
            Assert.AreEqual(ip1, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateFloatingIpWithNullNetworkIdThrows()
        {
            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            await client.CreateFloatingIp(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateFloatingIpWithEmptyNetworkIdThrows()
        {
            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            await client.CreateFloatingIp(string.Empty);
        }

        [TestMethod]
        public async Task CanDeleteFloatingIp()
        {
            var ipId = "12345";
            this.ServicePocoClient.DeleteFloatingIpDelegate = (ip) => Task.Factory.StartNew(() => Assert.AreEqual(ipId, ip));

            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            await client.DeleteFloatingIp(ipId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteFloatingIpWithNullFloatingIpIdThrows()
        {
            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            await client.DeleteFloatingIp(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteFloatingIpWithEmptyFloatingIpIdThrows()
        {
            var client = new NetworkServiceClient(GetValidCreds(), "Neutron", CancellationToken.None, this.ServiceLocator);
            await client.DeleteFloatingIp(string.Empty);
        }
    }
}
