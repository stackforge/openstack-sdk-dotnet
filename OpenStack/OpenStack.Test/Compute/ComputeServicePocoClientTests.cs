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

        private string GenerateMetadataPayload(IDictionary<string, string> metadata)
        {
            var payload = new StringBuilder();
            payload.Append("{ \"metadata\" : {");
            var isFirst = true;

            foreach (var item in metadata)
            {
                if (!isFirst)
                {
                    payload.Append(",");
                }

                payload.AppendFormat("\"{0}\":\"{1}\"", item.Key, item.Value);
                isFirst = false;
            }

            payload.Append("}}");
            return payload.ToString();
        }

        private IDictionary<string, string> ParseMetadataPayload(string payload)
        {
            var jObj = JObject.Parse(payload);
            var metaToken = jObj["metadata"];
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(metaToken.ToString());
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

        #region Get Compute Server Metadata Tests

        [TestMethod]
        public async Task CanGetComputeServerMetadataWithNonAuthoritativeResponse()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };
            var content = TestHelper.CreateStream(GenerateMetadataPayload(metadata));

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var respData = await client.GetServerMetadata("1");

            Assert.AreEqual(2, respData.Count);
            Assert.AreEqual("value1", respData["item1"]);
            Assert.AreEqual("value2", respData["item2"]);
        }

        [TestMethod]
        public async Task CanGetComputeServerMetadataWithOkResponse()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };
            var content = TestHelper.CreateStream(GenerateMetadataPayload(metadata));

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var respData = await client.GetServerMetadata("1");

            Assert.AreEqual(2, respData.Count);
            Assert.AreEqual("value1", respData["item1"]);
            Assert.AreEqual("value2", respData["item2"]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingComputeServerMetadataAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetServerMetadata("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingComputeServerMetadataAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetServerMetadata("12345");
        }

        #endregion

        #region Create Compute Server Tests

        [TestMethod]
        public async Task CanCreateComputeServerWithAcceptedResponse()
        {
            var serverId = "98765";
            var keyName = "MyKey";
            var publicUrl = "http://15.125.87.81:8774/v2/ffe683d1060449d09dac0bf9d7a371cd/servers/" + serverId;
            var permUrl = "http://15.125.87.81:8774/ffe683d1060449d09dac0bf9d7a371cd/servers/" + serverId;
            var adminPassword = "ABCDEF";
            var serverFixture = @"{{
                ""server"": {{
                    ""security_groups"": [
                        {{
                            ""name"": ""default""
                        }},
                        {{
                            ""name"": ""MyGroup""
                        }}
                    ],
                    ""OS-DCF:diskConfig"": ""MANUAL"",
                    ""id"": ""{0}"",
                    ""links"": [
                        {{
                            ""href"": ""{1}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{2}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""adminPass"": ""{3}""
                }}
            }}";

            var payload = string.Format(serverFixture, serverId, publicUrl, permUrl, adminPassword);
            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.Accepted);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.CreateServer("MyServer", "12345", "1", "54321", keyName, new List<string>() { "MyGroup" });

            Assert.IsNotNull(result);
            
            Assert.AreEqual(serverId, result.Id);
            Assert.AreEqual(adminPassword, result.AdminPassword);
            Assert.AreEqual(new Uri(publicUrl), result.PublicUri);
            Assert.AreEqual(new Uri(permUrl), result.PermanentUri);
        }

        [TestMethod]
        public async Task CanCreateComputeServerWithOkResponse()
        {
            var serverId = "98765";
            var keyName = "MyKey";
            var publicUrl = "http://15.125.87.81:8774/v2/ffe683d1060449d09dac0bf9d7a371cd/servers/" + serverId;
            var permUrl = "http://15.125.87.81:8774/ffe683d1060449d09dac0bf9d7a371cd/servers/" + serverId;
            var adminPassword = "ABCDEF";
            var serverFixture = @"{{
                ""server"": {{
                    ""security_groups"": [
                        {{
                            ""name"": ""default""
                        }},
                        {{
                            ""name"": ""MyGroup""
                        }}
                    ],
                    ""OS-DCF:diskConfig"": ""MANUAL"",
                    ""id"": ""{0}"",
                    ""links"": [
                        {{
                            ""href"": ""{1}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{2}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""adminPass"": ""{3}""
                }}
            }}";

            var payload = string.Format(serverFixture, serverId, publicUrl, permUrl, adminPassword);
            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.CreateServer("MyServer", "12345", "1", "54321", keyName, new List<string>() { "MyGroup" });

            Assert.IsNotNull(result);

            Assert.AreEqual(serverId, result.Id);
            Assert.AreEqual(adminPassword, result.AdminPassword);
            Assert.AreEqual(new Uri(publicUrl), result.PublicUri);
            Assert.AreEqual(new Uri(permUrl), result.PermanentUri);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenCreatingAComputeServerAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateServer("MyServer", "12345", "1", "54321", string.Empty, new List<string>() { "MyGroup" });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenCreatingAComputeServerAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateServer("MyServer", "12345", "1", "54321", string.Empty, new List<string>() { "MyGroup" });
        }

        #endregion

        #region Delete Compute Server Tests

        [TestMethod]
        public async Task CanDeleteComputeServerWithOkResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(),
                HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteServer("12345");
        }

        [TestMethod]
        public async Task CanDeleteComputeServerWithNoContentResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(),
                HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteServer("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingAComputeServerAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteServer("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingAComputeServerAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteServer("12345");
        }

        #endregion

        #region Get Compute Servers Tests

        public string ServersPayload = @"{
                            ""servers"": [
                                {
                                    ""id"": ""1"",
                                    ""links"": [
                                        {
                                            ""href"": ""http://someuri.com/v2/servers/1"",
                                            ""rel"": ""self""
                                        },
                                        {
                                            ""href"": ""http://someuri.com/servers/1"",
                                            ""rel"": ""bookmark""
                                        }
                                    ],
                                    ""name"": ""server1""
                                }
                            ]
                        }";

        [TestMethod]
        public async Task CanGetComputeServersWithOkResponse()
        {
            var content = TestHelper.CreateStream(ServersPayload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetServers();

            Assert.IsNotNull(result);

            var servers = result.ToList();
            Assert.AreEqual(1, servers.Count());

            var server = servers.First();
            Assert.AreEqual("server1", server.Name);
            Assert.AreEqual("1", server.Id);
            Assert.AreEqual(new Uri("http://someuri.com/v2/servers/1"), server.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/servers/1"), server.PermanentUri);
        }

        [TestMethod]
        public async Task CanGetComputeServersWithNonAuthoritativeResponse()
        {
            var content = TestHelper.CreateStream(ServersPayload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetServers();

            Assert.IsNotNull(result);

            var servers = result.ToList();
            Assert.AreEqual(1, servers.Count());

            var server = servers.First();
            Assert.AreEqual("server1", server.Name);
            Assert.AreEqual("1", server.Id);
            Assert.AreEqual(new Uri("http://someuri.com/v2/servers/1"), server.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/servers/1"), server.PermanentUri);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetComputeServersWithNoContent()
        {

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetServers();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingComputeServersAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetServers();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingComputeServersAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetServers();
        }

        #endregion

        #region Get Compute Server Tests

            public string ServerPayload = @"{
            ""server"": {
                ""status"": ""ACTIVE"",
                ""updated"": ""2014-06-11T18:04:46Z"",
                ""hostId"": ""bd5417ccb076908f6e0d639c37c053b0b6b9681db3464d19908dd4d9"",
                ""addresses"": {
                    ""private"": [
                        {
                            ""OS-EXT-IPS-MAC:mac_addr"": ""fa:16:3e:34:da:44"",
                            ""version"": 4,
                            ""addr"": ""10.0.0.2"",
                            ""OS-EXT-IPS:type"": ""fixed""
                        },
                        {
                            ""OS-EXT-IPS-MAC:mac_addr"": ""fa:16:3e:34:da:44"",
                            ""version"": 4,
                            ""addr"": ""172.24.4.3"",
                            ""OS-EXT-IPS:type"": ""floating""
                        }
                    ]
                },
                ""links"": [
                    {
                        ""href"": ""http://someuri.com/v2/servers/1"",
                        ""rel"": ""self""
                    },
                    {
                        ""href"": ""http://someuri.com/servers/1"",
                        ""rel"": ""bookmark""
                    }
                ],
                ""key_name"": null,
                ""image"": {
                    ""id"": ""c650e788-3c46-4efc-bfa6-1d94a14d6405"",
                    ""links"": [
                        {
                            ""href"": ""http://15.125.87.81:8774/ffe683d1060449d09dac0bf9d7a371cd/images/c650e788-3c46-4efc-bfa6-1d94a14d6405"",
                            ""rel"": ""bookmark""
                        }
                    ]
                },
                ""OS-EXT-STS:task_state"": null,
                ""OS-EXT-STS:vm_state"": ""active"",
                ""OS-SRV-USG:launched_at"": ""2014-06-11T18:04:45.000000"",
                ""flavor"": {
                    ""id"": ""1"",
                    ""links"": [
                        {
                            ""href"": ""http://15.125.87.81:8774/ffe683d1060449d09dac0bf9d7a371cd/flavors/1"",
                            ""rel"": ""bookmark""
                        }
                    ]
                },
                ""id"": ""1"",
                ""security_groups"": [
                    {
                        ""name"": ""MyGroup""
                    },
                    {
                        ""name"": ""default""
                    }
                ],
                ""OS-SRV-USG:terminated_at"": null,
                ""OS-EXT-AZ:availability_zone"": ""nova"",
                ""user_id"": ""70d48d344b494a1cbe8adbf7c02be7b5"",
                ""name"": ""wfoley1"",
                ""created"": ""2014-06-11T18:04:25Z"",
                ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                ""OS-DCF:diskConfig"": ""AUTO"",
                ""os-extended-volumes:volumes_attached"": [],
                ""accessIPv4"": """",
                ""accessIPv6"": """",
                ""progress"": 0,
                ""OS-EXT-STS:power_state"": 1,
                ""config_drive"": """",
                ""metadata"": {}
            }
        }";

        [TestMethod]
        public async Task CanGetComputeServerWithOkResponse()
        {
            var content = TestHelper.CreateStream(ServerPayload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetServer("1");

            Assert.IsNotNull(result);
            Assert.AreEqual("wfoley1", result.Name);
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual(ComputeServerStatus.Active, result.Status);
            Assert.AreEqual(0, result.Progress);
            Assert.AreEqual(new Uri("http://someuri.com/v2/servers/1"), result.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/servers/1"), result.PermanentUri);
        }

        [TestMethod]
        public async Task CanGetComputeServerWithNonAuthoritativeResponse()
        {
            var content = TestHelper.CreateStream(ServerPayload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetServer("1");

            Assert.IsNotNull(result);
            Assert.AreEqual("wfoley1", result.Name);
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual(ComputeServerStatus.Active, result.Status);
            Assert.AreEqual(0, result.Progress);
            Assert.AreEqual(new Uri("http://someuri.com/v2/servers/1"), result.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/servers/1"), result.PermanentUri);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetComputeServerWithNoContent()
        {

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetServer("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeServerAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetServer("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeServerAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetServer("1");
        }

        #endregion

        #region Assign Floating IP Tests

        [TestMethod]
        public async Task CanAssignFloatingIpWithOkResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.AssignFloatingIp("12345", "172.0.0.1");
        }

        [TestMethod]
        public async Task CanDAssignFloatingIpWithAcceptedResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Accepted);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.AssignFloatingIp("12345", "172.0.0.1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenAssigningAFloatingIpAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.AssignFloatingIp("12345", "172.0.0.1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhennAssigningAFloatingIpAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.AssignFloatingIp("12345", "172.0.0.1");
        }

        #endregion

        #region Update Compute Server Metadata Tests

        [TestMethod]
        public async Task CanUpdateComputeServerMetadataWithOkResponse()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateServerMetadata("1", metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenUpdatingComputeServerMetadataAndNotAuthed()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateServerMetadata("12345", metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenUpdatingComputeServerMetadataAndServerError()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateServerMetadata("12345", metadata);
        }

        #endregion

        #region Delete Compute Server Metadata Tests

        [TestMethod]
        public async Task CanDeleteComputeServerMetadataWithNoContentResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteServerMetadata("1", "item1");
        }

        [TestMethod]
        public async Task CanDeleteComputeServerMetadataWithOkResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteServerMetadata("1", "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingComputeServerMetadataAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteServerMetadata("1", "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingComputeServerMetadataAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteServerMetadata("1", "item1");
        }

        #endregion

        #region Get Compute Image Tests

        [TestMethod]
        public async Task CanGetComputeImageWithOkResponse()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var payload = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minRam"": 512,
                                        ""minDisk"": 10,
                                        ""progress"": 100,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetImage("12345");

            Assert.IsNotNull(result);
            Assert.AreEqual("image1", result.Name);
            Assert.AreEqual("ACTIVE", result.Status);
            Assert.AreEqual("12345", result.Id);
            Assert.AreEqual(512, result.MinimumRamSize);
            Assert.AreEqual(10, result.MinimumDiskSize);
            Assert.AreEqual(100, result.UploadProgress);
            Assert.AreEqual(created.ToLongTimeString(), result.CreateDate.ToLongTimeString());
            Assert.AreEqual(updated.ToLongTimeString(), result.LastUpdated.ToLongTimeString());
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), result.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), result.PublicUri);
        }

        [TestMethod]
        public async Task CanGetComputeImageWithNonAuthoritativeResponse()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var payload = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minRam"": 512,
                                        ""minDisk"": 10,
                                        ""progress"": 100,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetImage("12345");

            Assert.IsNotNull(result);
            Assert.AreEqual("image1", result.Name);
            Assert.AreEqual("ACTIVE", result.Status);
            Assert.AreEqual("12345", result.Id);
            Assert.AreEqual(512, result.MinimumRamSize);
            Assert.AreEqual(10, result.MinimumDiskSize);
            Assert.AreEqual(100, result.UploadProgress);
            Assert.AreEqual(created.ToLongTimeString(), result.CreateDate.ToLongTimeString());
            Assert.AreEqual(updated.ToLongTimeString(), result.LastUpdated.ToLongTimeString());
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), result.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), result.PublicUri);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetComputeImageWithNoContent()
        {

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetImage("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeImageAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetImage("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeImageAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetImage("1");
        }

        #endregion

        #region Get Compute Images Tests

        [TestMethod]
        public async Task CanGetComputeImagesWithOkResponse()
        {
            var payload = @"{
                            ""images"": [
                                {
                                    ""id"": ""12345"",
                                    ""links"": [
                                        {
                                            ""href"": ""http://someuri.com/v2/images/12345"",
                                            ""rel"": ""self""
                                        },
                                        {
                                            ""href"": ""http://someuri.com/images/12345"",
                                            ""rel"": ""bookmark""
                                        }
                                    ],
                                    ""name"": ""image1""
                                }
                            ]
                        }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetImages();

            Assert.IsNotNull(result);

            var images = result.ToList();
            Assert.AreEqual(1, images.Count());

            var image = images.First();
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
        }

        [TestMethod]
        public async Task CanGetComputeImagesWithNonAuthoritativeResponse()
        {
            var payload = @"{
                            ""images"": [
                                {
                                    ""id"": ""12345"",
                                    ""links"": [
                                        {
                                            ""href"": ""http://someuri.com/v2/images/12345"",
                                            ""rel"": ""self""
                                        },
                                        {
                                            ""href"": ""http://someuri.com/images/12345"",
                                            ""rel"": ""bookmark""
                                        }
                                    ],
                                    ""name"": ""image1""
                                }
                            ]
                        }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetImages();

            Assert.IsNotNull(result);

            var images = result.ToList();
            Assert.AreEqual(1, images.Count());

            var image = images.First();
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetComputeImagesWithNoContent()
        {

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetImages();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeImagesAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetImages();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeImagesAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetImages();
        }

        #endregion

        #region Delete Compute Image Tests

        [TestMethod]
        public async Task CanDeleteComputeImageWithNoContentResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteImage("12345");
        }

        [TestMethod]
        public async Task CanDeleteComputeImageWithOkResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteImage("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingAComputeImageAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteImage("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingAComputeImageAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteImage("12345");
        }

        #endregion

        #region Get Compute Image Metadata Tests

        [TestMethod]
        public async Task CanGetComputeImageMetadataWithNonAuthoritativeResponse()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };
            var content = TestHelper.CreateStream(GenerateMetadataPayload(metadata));

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var respData = await client.GetImageMetadata("12345");

            Assert.AreEqual(2, respData.Count);
            Assert.AreEqual("value1", respData["item1"]);
            Assert.AreEqual("value2", respData["item2"]);
        }

        [TestMethod]
        public async Task CanGetComputeImageMetadataWithOkResponse()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };
            var content = TestHelper.CreateStream(GenerateMetadataPayload(metadata));

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var respData = await client.GetImageMetadata("12345");

            Assert.AreEqual(2, respData.Count);
            Assert.AreEqual("value1", respData["item1"]);
            Assert.AreEqual("value2", respData["item2"]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingComputeImageMetadataAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetImageMetadata("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingComputeImageMetadataAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetImageMetadata("12345");
        }

        #endregion

        #region Update Compute Image Metadata Tests

        [TestMethod]
        public async Task CanUpdateComputeImageMetadataWithOkResponse()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateImageMetadata("12345", metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenUpdatingComputeImageMetadataAndNotAuthed()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateImageMetadata("12345", metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenUpdatingComputeImageMetadataAndServerError()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" }, { "item2", "value2" } };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateImageMetadata("12345", metadata);
        }

        #endregion

        #region Delete Compute Image Metadata Tests

        [TestMethod]
        public async Task CanDeleteComputeImageMetadataWithNoContentResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteImageMetadata("12345", "item1");
        }

        [TestMethod]
        public async Task CanDeleteComputeImageMetadataWithOkResponse()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteImageMetadata("1", "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingComputeImageMetadataAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteImageMetadata("1", "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDeletingComputeImageMetadataAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteImageMetadata("1", "item1");
        }

        #endregion

        #region Get Compute Key Pairs Tests

        public string KeyPairsPayload = @"{
            ""keypairs"": [
                {
                    ""keypair"": {
                        ""public_key"": ""ABCDEF"",
                        ""name"": ""MyKey"",
                        ""fingerprint"": ""12345""
                    }
                }
            ]
        }";

        [TestMethod]
        public async Task CanGetComputeKeyPairsWithOkResponse()
        {
            var content = TestHelper.CreateStream(KeyPairsPayload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetKeyPairs();

            Assert.IsNotNull(result);

            var pairs = result.ToList();
            Assert.AreEqual(1, pairs.Count());

            var keyPair = pairs.First();
            Assert.AreEqual("MyKey", keyPair.Name);
            Assert.AreEqual("ABCDEF", keyPair.PublicKey);
            Assert.AreEqual("12345", keyPair.Fingerprint);
        }

        [TestMethod]
        public async Task CanGetComputeKeyPairsWithNonAuthoritativeResponse()
        {
            var content = TestHelper.CreateStream(KeyPairsPayload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetKeyPairs();

            Assert.IsNotNull(result);

            var pairs = result.ToList();
            Assert.AreEqual(1, pairs.Count());

            var keyPair = pairs.First();
            Assert.AreEqual("MyKey", keyPair.Name);
            Assert.AreEqual("ABCDEF", keyPair.PublicKey);
            Assert.AreEqual("12345", keyPair.Fingerprint);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetComputeKeyPairsWithNoContent()
        {

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetKeyPairs();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingComputeKeyPairsAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetKeyPairs();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingComputeKeyPairsAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetKeyPairs();
        }

        #endregion

        #region Get Compute Key Pair Tests

        public string KeyPairPayload = @"{
            ""keypair"": {
                ""public_key"": ""ABCDEF"",
                ""user_id"": ""70d48d344b494a1cbe8adbf7c02be7b5"",
                ""name"": ""MyKey"",
                ""deleted"": false,
                ""created_at"": ""2014-08-11T21:15:53.000000"",
                ""updated_at"": null,
                ""fingerprint"": ""12345"",
                ""deleted_at"": null,
                ""id"": 1
            }
        }";

        [TestMethod]
        public async Task CanGetComputeKeyPairWithOkResponse()
        {
            var content = TestHelper.CreateStream(KeyPairPayload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var keyPair = await client.GetKeyPair("MyKey");

            Assert.IsNotNull(keyPair);
            Assert.AreEqual("MyKey", keyPair.Name);
            Assert.AreEqual("ABCDEF", keyPair.PublicKey);
            Assert.AreEqual("12345", keyPair.Fingerprint);
        }

        [TestMethod]
        public async Task CanGetComputeKeyPairWithNonAuthoritativeResponse()
        {
            var content = TestHelper.CreateStream(KeyPairPayload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            var keyPair = await client.GetKeyPair("MyKey");

            Assert.IsNotNull(keyPair);
            Assert.AreEqual("MyKey", keyPair.Name);
            Assert.AreEqual("ABCDEF", keyPair.PublicKey);
            Assert.AreEqual("12345", keyPair.Fingerprint);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetComputeKeyPairWithNoContent()
        {

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetKeyPair("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeKeyPairAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetKeyPair("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAComputeKeyPairAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.ComputeServiceRestClient.Responses.Enqueue(restResp);

            var client = new ComputeServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetKeyPair("1");
        }

        #endregion
    }
}
