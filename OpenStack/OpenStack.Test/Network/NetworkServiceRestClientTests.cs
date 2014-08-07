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
        internal Uri endpoint = new Uri("http://testnetworkendpoint.com/v2/1234567890");
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

            Assert.AreEqual(string.Format("{0}/networks", endpoint), this.simulator.Uri.ToString());
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
    }
}
