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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;
using OpenStack.Identity;
using OpenStack.Network;

namespace OpenStack.Test.Network
{
    [TestClass]
    public class NetworkServiceRestClientTests
    {
        internal NetworkRestSimulator simulator;
        internal string authId = "12345";
        internal Uri endpoint = new Uri("http://testnetworkendpoint.com");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.simulator = new NetworkRestSimulator();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IHttpAbstractionClientFactory), new NetworkRestSimulatorFactory(simulator));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.simulator = new NetworkRestSimulator();
            this.ServiceLocator = new ServiceLocator();
        }

        ServiceClientContext GetValidContext()
        {
            return GetValidContext(CancellationToken.None);
        }

        ServiceClientContext GetValidContext(CancellationToken token)
        {
            var creds = new OpenStackCredential(this.endpoint, "SomeUser", "Password", "SomeTenant", "region-a.geo-1");
            creds.SetAccessTokenId(this.authId);

            return new ServiceClientContext(creds, token, "Nova", endpoint);
        }

        #region Get Networks Test

        [TestMethod]
        public async Task GetNetworksIncludesAuthHeader()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetNetworks();

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetNetworksFormsCorrectUrlAndMethod()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetNetworks();

            Assert.AreEqual(string.Format("{0}/networks", endpoint +"v2.0"), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanGetNetworks()
        {
            this.simulator.Networks.Add(new OpenStack.Network.Network("12345","MyNetwork", NetworkStatus.Active));

            var client =
               new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetNetworks();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);
        }

        #endregion

        #region Get FloatingIps Tests

        [TestMethod]
        public async Task GetFloatingIpsIncludesAuthHeader()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFloatingIps();

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetFloatingIpsFormsCorrectUrlAndMethod()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFloatingIps();

            Assert.AreEqual(string.Format("{0}/floatingips", endpoint + "v2.0"), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanGetFloatingIps()
        {
            this.simulator.FloatingIps.Add(new OpenStack.Network.FloatingIp("12345", "172.0.0.1", FloatingIpStatus.Active));

            var client =
               new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetFloatingIps();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);
        }

        #endregion

        #region Get FloatingIp Tests

        [TestMethod]
        public async Task GetFloatingIpIncludesAuthHeader()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetNetworks();

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetFloatingIpFormsCorrectUrlAndMethod()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFloatingIp("12345");

            Assert.AreEqual(string.Format("{0}/floatingips/12345", endpoint + "v2.0"), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanGetFloatinIp()
        {
            this.simulator.FloatingIps.Add(new OpenStack.Network.FloatingIp("12345", "172.0.0.1", FloatingIpStatus.Active));

            var client =
               new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetFloatingIp("12345");

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);
        }

        #endregion

        #region Create FloatingIp Tests

        [TestMethod]
        public async Task CreateFloatingIpIncludesAuthHeader()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CreateFloatingIp("12345");

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task CreateFloatingIpFormsCorrectUrlAndMethod()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CreateFloatingIp("12345");

            Assert.AreEqual(string.Format("{0}/floatingips", endpoint + "v2.0"), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Post, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanCreateFloatinIp()
        {
            var client =
               new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CreateFloatingIp("12345");

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);
        }

        [TestMethod]
        public async Task CreateFloatinIpFormCorrectBody()
        {
            var client =
               new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CreateFloatingIp("12345");

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            this.simulator.Content.Position = 0;
            var body = TestHelper.GetStringFromStream(this.simulator.Content);
            var ipObj = JObject.Parse(body);
            Assert.IsNotNull(ipObj);
            Assert.IsNotNull(ipObj["floatingip"]);
            Assert.IsNotNull(ipObj["floatingip"]["floating_network_id"]);
            Assert.AreEqual("12345", (string)ipObj["floatingip"]["floating_network_id"]);
        }

        #endregion

        #region Delete Floating Ip Tests

        [TestMethod]
        public async Task DeleteFloatingIpIncludesAuthHeader()
        {
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.DeleteFloatingIp("12345");

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task DeleteFloatingIpFormsCorrectUrlAndMethod()
        {
            var floatingIpId = "12345";
            var client =
                new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.DeleteFloatingIp(floatingIpId);

            Assert.AreEqual(string.Format("{0}/floatingips/{1}", endpoint + "v2.0", floatingIpId), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Delete, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanDeleteFloatinIp()
        {
            this.simulator.FloatingIps.Add(new FloatingIp("12345", "172.0.0.1", FloatingIpStatus.Active));
            var client =
               new NetworkServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.DeleteFloatingIp("12345");

            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            Assert.AreEqual(0, this.simulator.FloatingIps.Count);
        }

        #endregion
    }
}
