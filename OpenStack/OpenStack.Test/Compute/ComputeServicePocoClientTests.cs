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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;
using OpenStack.Identity;

namespace OpenStack.Test.Compute
{
    [TestClass]
    public class ComputeServicePocoClientTests
    {
        internal TestComputeServiceRestClient ComputeServiceRestClient;
        internal string authId = "12345";
        internal Uri endpoint = new Uri("http://testcomputeendpoint.com/v1/1234567890");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.ComputeServiceRestClient = new TestComputeServiceRestClient();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IComputeServiceRestClientFactory), new TestComputeServiceRestClientFactory(ComputeServiceRestClient));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.ComputeServiceRestClient = new TestComputeServiceRestClient();
            this.ServiceLocator = new ServiceLocator();
        }

        ServiceClientContext GetValidContext()
        {
            var creds = new OpenStackCredential(this.endpoint, "SomeUser", "Password", "SomeTenant", "region-a.geo-1");
            creds.SetAccessTokenId(this.authId);

            return new ServiceClientContext(creds, CancellationToken.None, "Object Storage", endpoint);
        }
         
        #region Get Compute Flavor Tests

        [TestMethod]
        public async Task CanGetComputeFlavorWithOkResponse()
        {
            var payload = @"{
                            ""flavor"": {
                                ""name"": ""m1.tiny"",
                                ""id"": ""1"",
                                ""links"": [
                                    {
                                        ""href"": ""http://someuri.com/v2/flavors/1"",
                                        ""rel"": ""self""
                                    },
                                    {
                                        ""href"": ""http://someuri.com/flavors/1"",
                                        ""rel"": ""bookmark""
                                    }
                                ],
                                ""ram"" : 512,
                                ""vcpus"": 2,
                                ""disk"": 10
                            }
                        }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetFlavor("1");

            Assert.IsNotNull(result);
            Assert.AreEqual("m1.tiny", result.Name);
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("512", result.Ram);
            Assert.AreEqual("2", result.Vcpus);
            Assert.AreEqual("10", result.Disk);
            Assert.AreEqual(new Uri("http://someuri.com/v2/flavors/1"), result.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/flavors/1"), result.PermanentUri);
        }

        [TestMethod]
        public async Task CanGetComputeFlavorWithNonAuthoritativeResponse()
        {
            var payload = @"{
                            ""flavor"": {
                                ""name"": ""m1.tiny"",
                                ""id"": ""1"",
                                ""links"": [
                                    {
                                        ""href"": ""http://someuri.com/v2/flavors/1"",
                                        ""rel"": ""self""
                                    },
                                    {
                                        ""href"": ""http://someuri.com/flavors/1"",
                                        ""rel"": ""bookmark""
                                    }
                                ],
                                ""ram"" : 512,
                                ""vcpus"": 2,
                                ""disk"": 10
                            }
                        }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetFlavor("1");

            Assert.IsNotNull(result);
            Assert.AreEqual("m1.tiny", result.Name);
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("512", result.Ram);
            Assert.AreEqual("2", result.Vcpus);
            Assert.AreEqual("10", result.Disk);
            Assert.AreEqual(new Uri("http://someuri.com/v2/flavors/1"), result.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/flavors/1"), result.PermanentUri);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetComputeFlavorWithNoContent()
        {
            
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFlavor("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeFlavorAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFlavor("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeFlavorAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFlavor("1");
        }

        #endregion

        #region Get Compute Flavors Tests

        [TestMethod]
        public async Task CanGetComputeFlavorsWithOkResponse()
        {
            var payload = @"{
                            ""flavors"": [
                                {
                                    ""id"": ""1"",
                                    ""links"": [
                                        {
                                            ""href"": ""http://someuri.com/v2/flavors/1"",
                                            ""rel"": ""self""
                                        },
                                        {
                                            ""href"": ""http://someuri.com/flavors/1"",
                                            ""rel"": ""bookmark""
                                        }
                                    ],
                                    ""name"": ""m1.tiny""
                                }
                            ]
                        }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetFlavors();

            Assert.IsNotNull(result);
            
            var flavors = result.ToList();
            Assert.AreEqual(1, flavors.Count());

            var flavor = flavors.First();
            Assert.AreEqual("m1.tiny", flavor.Name);
            Assert.AreEqual("1", flavor.Id);
            Assert.AreEqual(new Uri("http://someuri.com/v2/flavors/1"), flavor.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/flavors/1"), flavor.PermanentUri);
        }

        [TestMethod]
        public async Task CanGetComputeFlavorsWithNonAuthoritativeResponse()
        {
            var payload = @"{
                            ""flavors"": [
                                {
                                    ""id"": ""1"",
                                    ""links"": [
                                        {
                                            ""href"": ""http://someuri.com/v2/flavors/1"",
                                            ""rel"": ""self""
                                        },
                                        {
                                            ""href"": ""http://someuri.com/flavors/1"",
                                            ""rel"": ""bookmark""
                                        }
                                    ],
                                    ""name"": ""m1.tiny""
                                }
                            ]
                        }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetFlavors();

            Assert.IsNotNull(result);

            var flavors = result.ToList();
            Assert.AreEqual(1, flavors.Count());

            var flavor = flavors.First();
            Assert.AreEqual("m1.tiny", flavor.Name);
            Assert.AreEqual("1", flavor.Id);
            Assert.AreEqual(new Uri("http://someuri.com/v2/flavors/1"), flavor.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/flavors/1"), flavor.PermanentUri);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetComputeFlavorsWithNoContent()
        {

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFlavors();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeFlavorsAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFlavors();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeFlavorsAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetFlavors();
        }

        #endregion
    }
}
