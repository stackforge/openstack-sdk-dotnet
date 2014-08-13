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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;
using OpenStack.Network;

namespace OpenStack.Test.Network
{
    [TestClass]
    public class NetworkServicePocoClientTests
    {
        internal TestNetworkServiceRestClient NetworkServiceRestClient;
        internal string authId = "12345";
        internal Uri endpoint = new Uri("http://testnetworkendpoint.com/v2.0/1234567890");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.NetworkServiceRestClient = new TestNetworkServiceRestClient();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(INetworkServiceRestClientFactory), new TestNetworkServiceRestClientFactory(this.NetworkServiceRestClient));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.NetworkServiceRestClient = new TestNetworkServiceRestClient();
            this.ServiceLocator = new ServiceLocator();
        }

        ServiceClientContext GetValidContext()
        {
            var creds = new OpenStackCredential(this.endpoint, "SomeUser", "Password", "SomeTenant", "region-a.geo-1");
            creds.SetAccessTokenId(this.authId);

            return new ServiceClientContext(creds, CancellationToken.None, "Object Storage", endpoint);
        }

        #region Get Networks Tests

        [TestMethod]
        public async Task CanGetNetworksWithOkResponse()
        {
            var payload = @"{
                ""networks"": [
                    {
                        ""status"": ""ACTIVE"",
                        ""subnets"": [
                            ""d3839504-ec4c-47a4-b7c7-07af079a48bb""
                        ],
                        ""name"": ""myNetwork"",
                        ""router:external"": false,
                        ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                        ""admin_state_up"": true,
                        ""shared"": false,
                        ""id"": ""12345""
                    }
                ]
            }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetNetworks();

            Assert.IsNotNull(result);

            var networks = result.ToList();
            Assert.AreEqual(1, networks.Count());

            var network = networks.First();
            Assert.AreEqual("myNetwork", network.Name);
            Assert.AreEqual("12345", network.Id);
            Assert.AreEqual(NetworkStatus.Active, network.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetNetworksWithNoContent()
        {

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetNetworks();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingNetworksAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetNetworks();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingNetworksAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetNetworks();
        }

        #endregion

        #region Get Floating IPs Tests

        [TestMethod]
        public async Task CanGetFloatingIpsWithOkResponse()
        {
            var payload = @"{
                ""floatingips"": [
                    {
                        ""router_id"": ""fafac59b-a94a-4525-8700-f4f448e0ac97"",
                        ""status"": ""ACTIVE"",
                        ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                        ""floating_network_id"": ""3eaab3f7-d3f2-430f-aa73-d07f39aae8f4"",
                        ""fixed_ip_address"": ""10.0.0.2"",
                        ""floating_ip_address"": ""172.0.0.1"",
                        ""port_id"": ""9da94672-6e6b-446c-9579-3dd5484b31fd"",
                        ""id"": ""12345""
                    }
                ]
            }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetFloatingIps();

            Assert.IsNotNull(result);

            var floatingIps = result.ToList();
            Assert.AreEqual(1, floatingIps.Count());

            var floatingIp = floatingIps.First();
            Assert.AreEqual("12345", floatingIp.Id);
            Assert.AreEqual("172.0.0.1", floatingIp.FloatingIpAddress);
            Assert.AreEqual(FloatingIpStatus.Active, floatingIp.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingFloatingIpsAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFloatingIps();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingFloatingIpsAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFloatingIps();
        }

        #endregion

        #region Get Floating IP Tests

        [TestMethod]
        public async Task CanGetFloatingIpWithOkResponse()
        {
            var payload = @"{
                ""floatingip"":
                    {
                        ""router_id"": ""fafac59b-a94a-4525-8700-f4f448e0ac97"",
                        ""status"": ""ACTIVE"",
                        ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                        ""floating_network_id"": ""3eaab3f7-d3f2-430f-aa73-d07f39aae8f4"",
                        ""fixed_ip_address"": ""10.0.0.2"",
                        ""floating_ip_address"": ""172.0.0.1"",
                        ""port_id"": ""9da94672-6e6b-446c-9579-3dd5484b31fd"",
                        ""id"": ""12345""
                    }
            }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetFloatingIp("12345");

            Assert.IsNotNull(result);

            Assert.AreEqual("12345", result.Id);
            Assert.AreEqual("172.0.0.1", result.FloatingIpAddress);
            Assert.AreEqual(FloatingIpStatus.Active, result.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingFloatingIpAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFloatingIp("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingFloatingIpAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFloatingIp("12345");
        }

        #endregion

        #region Create Floating IP Tests

        [TestMethod]
        public async Task CanCreateFloatingIpWithCreatedResponse()
        {
            var payload = @"{
                ""floatingip"":
                    {
                        ""router_id"": ""fafac59b-a94a-4525-8700-f4f448e0ac97"",
                        ""status"": ""ACTIVE"",
                        ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                        ""floating_network_id"": ""3eaab3f7-d3f2-430f-aa73-d07f39aae8f4"",
                        ""fixed_ip_address"": ""10.0.0.2"",
                        ""floating_ip_address"": ""172.0.0.1"",
                        ""port_id"": ""9da94672-6e6b-446c-9579-3dd5484b31fd"",
                        ""id"": ""12345""
                    }
            }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.Created);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.CreateFloatingIp("12345");

            Assert.IsNotNull(result);

            Assert.AreEqual("12345", result.Id);
            Assert.AreEqual("172.0.0.1", result.FloatingIpAddress);
            Assert.AreEqual(FloatingIpStatus.Active, result.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenCreatingFloatingIpAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateFloatingIp("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenCreatingFloatingIpAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateFloatingIp("12345");
        }

        #endregion

        #region Delete Floating IP Tests

        [TestMethod]
        public async Task CanDeleteFloatingIpWithNoContentResponse()
        {
            
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteFloatingIp("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingFloatingIpAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteFloatingIp("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingFloatingIpAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.NetworkServiceRestClient.Responses.Enqueue(restResp);

            var client = new NetworkServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteFloatingIp("12345");
        }

        #endregion

    }
}
