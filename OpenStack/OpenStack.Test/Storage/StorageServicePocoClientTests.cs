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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class StorageServicePocoClientTests
    {
        internal TestStorageServiceRestClient StorageServiceRestClient;
        internal string authId = "12345";
        internal Uri endpoint = new Uri("http://teststorageendpoint.com/v1/1234567890");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.StorageServiceRestClient = new TestStorageServiceRestClient();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IStorageServiceRestClientFactory), new TestStorageServiceRestClientFactory(StorageServiceRestClient));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.StorageServiceRestClient = new TestStorageServiceRestClient();
            this.ServiceLocator = new ServiceLocator();
        }

        ServiceClientContext GetValidContext()
        {
            var creds = new OpenStackCredential(this.endpoint, "SomeUser", "Password", "SomeTenant", "region-a.geo-1");
            creds.SetAccessTokenId(this.authId);

            return new ServiceClientContext(creds, CancellationToken.None, "Object Storage", endpoint);
        }

        #region Get Storage Container Tests

        [TestMethod]
        public async Task CanGetStorageContainerWithOkResponse()
        {
            var containerName = "TestContainer";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Container-Bytes-Used", "1234"},
                {"X-Container-Object-Count", "1"}
            };

            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            var result = await client.GetStorageContainer(containerName);
            
            Assert.IsNotNull(result);
            Assert.AreEqual(containerName, result.Name);
            Assert.AreEqual(1234, result.TotalBytesUsed);
            Assert.AreEqual(1, result.TotalObjectCount);
            Assert.IsNotNull(result.Objects);
            Assert.AreEqual(1, result.Objects.Count());
        }

        [TestMethod]
        public async Task CanGetStorageContainerWithNoContent()
        {
            var containerName = "TestContainer";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Container-Bytes-Used", "0"},
                {"X-Container-Object-Count", "0"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetStorageContainer(containerName);

            Assert.IsNotNull(result);
            Assert.AreEqual(containerName, result.Name);
            Assert.AreEqual(0, result.TotalBytesUsed);
            Assert.AreEqual(0, result.TotalObjectCount);
            Assert.IsNotNull(result.Objects);
            Assert.AreEqual(0, result.Objects.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageContainerThatDoesNotExist()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NotFound);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageContainerAndNotAuthed()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageContainerAndServerError()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageContainerWithNullName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageContainer(null);
        }

        #endregion

        #region Get Storage Account Tests

        [TestMethod]
        public async Task CanGetStorageAccountWithOkResponse()
        {
            var accountName = "1234567890";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Account-Bytes-Used", "1234"},
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "1"}
            };

            var payload = @"[
                            {
                                    ""count"": 1,
                                    ""bytes"": 7,
                                    ""name"": ""TestContainer""
                            }]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetStorageAccount();

            Assert.IsNotNull(result);
            Assert.AreEqual(accountName, result.Name);
            Assert.AreEqual(1234, result.TotalBytesUsed);
            Assert.AreEqual(1, result.TotalObjectCount);
            Assert.AreEqual(1, result.TotalContainerCount);
            Assert.IsNotNull(result.Containers);
            Assert.AreEqual(1, result.Containers.Count());
        }

        [TestMethod]
        public async Task CanGetStorageAccontWithNoContent()
        {
            var accountName = "1234567890";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Account-Bytes-Used", "1234"},
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "1"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetStorageAccount();

            Assert.IsNotNull(result);
            Assert.AreEqual(accountName, result.Name);
            Assert.AreEqual(1234, result.TotalBytesUsed);
            Assert.AreEqual(1, result.TotalObjectCount);
            Assert.AreEqual(1, result.TotalContainerCount);
            Assert.IsNotNull(result.Containers);
            Assert.AreEqual(0, result.Containers.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageAccountAndNotAuthed()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageAccount();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageAccountAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageAccount();
        }

        #endregion

        #region Download Storage Object Tests

        [TestMethod]
        public async Task CanDownloadStorageObjectWithOkResponse()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var data = "some data";
            var content = TestHelper.CreateStream(data);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);

            var respContent = new MemoryStream();
            var result = await client.DownloadStorageObject(containerName, objectName, respContent);

            Assert.IsNotNull(result);
            Assert.AreEqual(objectName, result.Name);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(1234, result.Length);
            Assert.AreEqual("application/octet-stream", result.ContentType);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", result.ETag);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), result.LastModified);
            Assert.AreEqual(data,TestHelper.GetStringFromStream(respContent));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDownloadingAStorageObjectThatDoesNotExist()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NotFound);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DownloadStorageObject(containerName, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDownloadingAStorageObjectAndNotAuthed()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DownloadStorageObject(containerName, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDownloadingAStorageObjectAndServerError()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DownloadStorageObject(containerName, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotDownloadStorageObjectWithNullContainerName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DownloadStorageObject(null, "object", new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotDownloadStorageObjectWithNullObjectName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DownloadStorageObject("container", null, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotDownloadStorageObjectWithEmptyContainerName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DownloadStorageObject(string.Empty, "object", new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotDownloadStorageObjectWithEmptyObjectName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DownloadStorageObject("container", string.Empty, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotDownloadStorageObjectWithnullOutputStream()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DownloadStorageObject("container", "object", null);
        }

        #endregion

        #region Get Storage Object Tests

        [TestMethod]
        public async Task CanGetStorageObjectWithOkResponse()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetStorageObject(containerName, objectName);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result,typeof(StorageObject));
            Assert.IsNotInstanceOfType(result, typeof(StorageManifest));
            Assert.AreEqual(objectName, result.Name);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(1234, result.Length);
            Assert.AreEqual("application/octet-stream", result.ContentType);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", result.ETag);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), result.LastModified);
        }

        [TestMethod]
        public async Task CanGetStorageObjectWithOkResponseThatIsAManifest()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Static-Large-Object","True"}
            };

            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 54321,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetStorageObject(containerName, objectName);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StaticLargeObjectManifest));
            Assert.AreEqual(objectName, result.Name);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(1234, result.Length);
            Assert.AreEqual("application/octet-stream", result.ContentType);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", result.ETag);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), result.LastModified);
        }

        [TestMethod]
        public async Task CanGetStorageObjectWithNoContent()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetStorageObject(containerName, objectName);

            Assert.IsNotNull(result);
            Assert.AreEqual(objectName, result.Name);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(1234, result.Length);
            Assert.AreEqual("application/octet-stream", result.ContentType);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", result.ETag);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), result.LastModified);
        }

        [TestMethod]
        public async Task CanGetStorageObjectWithHeaders()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.GetStorageObject(containerName, objectName);

            Assert.IsNotNull(result);
            Assert.AreEqual(objectName, result.Name);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(1234, result.Length);
            Assert.AreEqual("application/octet-stream", result.ContentType);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", result.ETag);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), result.LastModified);
            Assert.AreEqual(1, result.Metadata.Count());
            Assert.IsTrue(result.Metadata.ContainsKey("Test1"));
            Assert.AreEqual("Test1", result.Metadata["Test1"]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageObjectThatDoesNotExist()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NotFound);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageObjectAndNotAuthed()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageObjectAndServerError()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageObjectWithNullContainerName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageObject(null,"object");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageObjectWithNullObjectName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageObject("container", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetStorageObjectWithEmptyContainerName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageObject(string.Empty, "object");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetStorageObjectWithEmptyObjectName()
        {
            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageObject("container", string.Empty);
        }

        #endregion

        #region Get Storage Manifest Tests

        [TestMethod]
        public async Task CanGetStaticStorageManifestWithOkResponseAndPayload()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/c/manifest";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Static-Large-Object","True"}
            };

            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            var result = await client.GetStorageManifest(containerName, manifestName);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StaticLargeObjectManifest));
            Assert.AreEqual(manifestName, result.FullName);
            Assert.AreEqual("manifest", result.Name);

            var manifest = result as StaticLargeObjectManifest;
            Assert.IsNotNull(manifest.Objects);
            Assert.AreEqual(1, manifest.Objects.Count());
            Assert.AreEqual("a/b/c/BLAH", manifest.Objects.First().FullName);
        }

        [TestMethod]
        public async Task CanGetDynamicStorageManifestWithOkResponseAndPayload()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/c/manifest";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Object-Manifest","a/b"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            var result = await client.GetStorageManifest(containerName, manifestName);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicLargeObjectManifest));
            Assert.AreEqual(manifestName, result.FullName);
            Assert.AreEqual("manifest", result.Name);
            var manifest = result as DynamicLargeObjectManifest;
            Assert.AreEqual("a/b", manifest.SegmentsPath);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetStaticStorageManifestWhenObjectIsNotManifest()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/c/manifest";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Static-Large-Object","False"}
            };

            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageManifest(containerName, manifestName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageManifestThatDoesNotExist()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/b/manifest";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NotFound);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageManifest(containerName, manifestName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStoragManifestAndNotAuthed()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/b/manifest";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageManifest(containerName, manifestName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageManifestAndServerError()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/b/manifest";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageManifest(containerName, manifestName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageManifestWithNullContainerName()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageManifest(null, "a/b/c/manifest");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageManifestWithNullFolderName()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageManifest("container", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetStorageManifestWithEmptyContainerName()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageManifest(string.Empty, "a/b/c/manifest");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetStorageManifestWithEmptyFolderName()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageManifest("container", string.Empty);
        }

        #endregion

        #region Get Storage Folder Tests

        [TestMethod]
        public async Task CanGetStorageFolderWithOkResponseAndNoSubFolders()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Container-Bytes-Used", "1234"},
                {"X-Container-Object-Count", "1"}
            };

            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            var result = await client.GetStorageFolder(containerName, folderName);

            Assert.IsNotNull(result);
            Assert.AreEqual("a/b/c/", result.FullName);
            Assert.AreEqual("c", result.Name);
            Assert.IsNotNull(result.Objects);
            Assert.AreEqual(1, result.Objects.Count());
            Assert.IsNotNull(result.Folders);
            Assert.AreEqual(0, result.Folders.Count());
        }

        [TestMethod]
        public async Task CanGetStorageFolderWithNoContentResponse()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Container-Bytes-Used", "1234"},
                {"X-Container-Object-Count", "1"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            var result = await client.GetStorageFolder(containerName, folderName);

            Assert.IsNotNull(result);
            Assert.AreEqual("a/b/c/", result.FullName);
            Assert.AreEqual("c", result.Name);
            Assert.IsNotNull(result.Objects);
            Assert.AreEqual(0, result.Objects.Count());
            Assert.IsNotNull(result.Folders);
            Assert.AreEqual(0, result.Folders.Count());
        }

        [TestMethod]
        public async Task CanGetStorageFolderWithOkResponseAndSubFolders()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Container-Bytes-Used", "1234"},
                {"X-Container-Object-Count", "1"}
            };

            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/"",
                                    ""content_type"": ""application/octet-stream""
                                },
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                },
                                {
                                        ""subdir"": ""a/b/c/d/""
                                },
                                {
                                        ""subdir"": ""a/b/c/x/""
                                }
                            ]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            var resp = await client.GetStorageFolder(containerName, folderName);

            Assert.AreEqual("c", resp.Name);
            Assert.AreEqual("a/b/c/", resp.FullName);
            Assert.AreEqual(1, resp.Objects.Count);
            Assert.AreEqual(2, resp.Folders.Count);

            var obj = resp.Objects.First();
            Assert.AreEqual("a/b/c/BLAH", obj.FullName);

            var dNode = resp.Folders.First(f => f.FullName == "a/b/c/d/");
            var xNode = resp.Folders.First(f => f.FullName == "a/b/c/x/");

            Assert.AreEqual("d", dNode.Name);
            Assert.AreEqual(0, dNode.Folders.Count);
            Assert.AreEqual(0, dNode.Objects.Count);

            Assert.AreEqual("x", xNode.Name);
            Assert.AreEqual(0, xNode.Folders.Count);
            Assert.AreEqual(0, xNode.Objects.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageFolderThatDoesNotExistAndCannotBeInferred()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Container-Bytes-Used", "1234"},
                {"X-Container-Object-Count", "1"}
            };

            var payload = @"[]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            var resp = await client.GetStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageFolderThatDoesNotExist()
        {
            var containerName = "TestContainer";
            var fodlerName = "a/b/b/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NotFound);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageFolder(containerName, fodlerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStoragFolderAndNotAuthed()
        {
            var containerName = "TestContainer";
            var fodlerName = "a/b/b/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageFolder(containerName, fodlerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageFolderAndServerError()
        {
            var containerName = "TestContainer";
            var fodlerName = "a/b/b/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.GetStorageFolder(containerName, fodlerName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageFolderWithNullContainerName()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageFolder(null, "a/b/c/");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageFolderWithNullFolderName()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageFolder("container", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetStorageFolderWithEmptyContainerName()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageFolder(string.Empty, "a/b/c/");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetStorageFolderWithEmptyFolderName()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;
            await client.GetStorageFolder("container", string.Empty);
        }

        #endregion

        #region Create Storage Manifest Tests

        [TestMethod]
        public async Task CanCreateStaticManifestWithCreatedResponse()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/c/manifest";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var headers2 = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Static-Large-Object","True"}
            };

            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var content = TestHelper.CreateStream(payload);

            var restResp1 = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.Created);
            var restResp2 = new HttpResponseAbstraction(content, headers2, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp1);
            this.StorageServiceRestClient.Responses.Enqueue(restResp2);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;

            var manifest = new StaticLargeObjectManifest(containerName, manifestName,
                new List<StorageObject>()
                {
                    new StorageObject("a/b/c/BLAH", containerName, new Dictionary<string, string>())
                });

            var result = await client.CreateStorageManifest(manifest);

            Assert.IsTrue(this.StorageServiceRestClient.CreateStaticManifestCalled);
            Assert.IsInstanceOfType(result, typeof(StaticLargeObjectManifest));
            Assert.AreEqual(manifestName, result.FullName);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(1, ((StaticLargeObjectManifest)result).Objects.Count);
            Assert.AreEqual("a/b/c/BLAH", ((StaticLargeObjectManifest)result).Objects.First().FullName);
        }

        [TestMethod]
        public async Task CanCreateDynamicManifestWithCreatedResponse()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/c/manifest";
            var segPath = "TestContainer/a/b/c";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var headers2 = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Manifest",segPath}
            };

            var restResp1 = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.Created);
            var restResp2 = new HttpResponseAbstraction(new MemoryStream(), headers2, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp1);
            this.StorageServiceRestClient.Responses.Enqueue(restResp2);

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;

            var manifest = new DynamicLargeObjectManifest(containerName, manifestName, segPath);
                
            var result = await client.CreateStorageManifest(manifest);

            Assert.IsTrue(this.StorageServiceRestClient.CreatedDynamicManifestCalled);
            Assert.IsInstanceOfType(result, typeof(DynamicLargeObjectManifest));
            Assert.AreEqual(manifestName, result.FullName);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(segPath, ((DynamicLargeObjectManifest)result).SegmentsPath);
        }

       

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotCreateManifestWithUnknownManifestType()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/c/manifest";

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;

            var manifest = new TestStorageManifest(containerName, manifestName);

            await client.CreateStorageManifest(manifest);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateManifestWithNullContainerName()
        {
            var manifestName = "a/b/c/manifest";

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;

            var manifest = new TestStorageManifest(null, manifestName);

            await client.CreateStorageManifest(manifest);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateManifestWithEmptyContainerName()
        {
            var manifestName = "a/b/c/manifest";

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;

            var manifest = new TestStorageManifest(string.Empty, manifestName);

            await client.CreateStorageManifest(manifest);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateManifestWithNullManifestName()
        {
            var containerName = "TestContainer";

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;

            var manifest = new TestStorageManifest(containerName, null);

            await client.CreateStorageManifest(manifest);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateManifestWithEmptyManifestName()
        {
            var containerName = "TestContainer";

            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;

            var manifest = new TestStorageManifest(containerName, string.Empty);

            await client.CreateStorageManifest(manifest);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateManifestWithNullManifest()
        {
            var client = new StorageServicePocoClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServicePocoClient;

           await client.CreateStorageManifest(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageManifestWithBadAuth()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/c/manifest";
            var segPath = "TestContainer/a/b/c";

            var manifest = new DynamicLargeObjectManifest(containerName, manifestName, segPath);

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageManifest(manifest);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageManifestHasInternalServerError()
        {
            var containerName = "TestContainer";
            var manifestName = "a/b/c/manifest";
            var segPath = "TestContainer/a/b/c";

            var manifest = new DynamicLargeObjectManifest(containerName, manifestName, segPath);

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageManifest(manifest);
        }

        #endregion

        #region Create Storage Object Tests

        [TestMethod]
        public async Task CanCreateStorageObjectWithCreatedResponse()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var objRequest = new StorageObject(objectName, containerName);
            var content = TestHelper.CreateStream("Some Content");

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.Created);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.CreateStorageObject(objRequest, content);

            Assert.IsNotNull(result);
            Assert.AreEqual(objectName, result.Name);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(1234, result.Length);
            Assert.AreEqual("application/octet-stream", result.ContentType);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", result.ETag);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), result.LastModified);
        }

        [TestMethod]
        public async Task CanCreateStorageObjectWithFoldersAndCreatedResponse()
        {
            var containerName = "TestContainer";
            var objectName = "a/b/TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var objRequest = new StorageObject(objectName, containerName);
            var content = TestHelper.CreateStream("Some Content");

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.Created);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            var result = await client.CreateStorageObject(objRequest, content);

            Assert.IsNotNull(result);
            Assert.AreEqual(objectName, result.FullName);
            Assert.AreEqual(containerName, result.ContainerName);
            Assert.AreEqual(1234, result.Length);
            Assert.AreEqual("application/octet-stream", result.ContentType);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", result.ETag);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), result.LastModified);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageObjectMissingLength()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objRequest = new StorageObject(objectName, containerName);
            var content = TestHelper.CreateStream("Some Content");

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.LengthRequired);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageObject(objRequest, content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageObjectWithBadETag()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objRequest = new StorageObject(objectName, containerName);
            var content = TestHelper.CreateStream("Some Content");

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), (HttpStatusCode)422);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageObject(objRequest, content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageObjectWithBadAuth()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objRequest = new StorageObject(objectName, containerName);
            var content = TestHelper.CreateStream("Some Content");

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageObject(objRequest, content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageObjectHasInternalServerError()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objRequest = new StorageObject(objectName, containerName);
            var content = TestHelper.CreateStream("Some Content");

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageObject(objRequest, content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageObjectTimesOut()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objRequest = new StorageObject(objectName, containerName);
            var content = TestHelper.CreateStream("Some Content");

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.RequestTimeout);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageObject(objRequest, content);
        }

        #endregion

        #region Create Storage Folder Tests

        [TestMethod]
        public async Task CanCreateStorageFolderWithCreatedResponse()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/b/";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.Created);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageFolderMissingLength()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/b/";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.LengthRequired);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageFolderWithBadETag()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/b/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), (HttpStatusCode)422);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageFolderWithBadAuth()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/b/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageFolderHasInternalServerError()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/b/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageFolderTimesOut()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/b/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.RequestTimeout);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        #endregion

        #region Create Storage Container Tests

        [TestMethod]
        public async Task CanCreateStorageContainerWithCreatedResponse()
        {
            var containerName = "TestContainer";
            
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "1"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.Created);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageContainer(containerReq);

            //Assert.IsNotNull(container);
            //Assert.AreEqual(containerName, container.Name);
            //Assert.AreEqual(12345, container.TotalBytesUsed);
        }

        [TestMethod]
        public async Task CanCreateStorageContainerWithNoContentResponse()
        {
            var containerName = "TestContainer";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "1"}
            };

            var restResp = new HttpResponseAbstraction(new MemoryStream(), headers, HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageContainer(containerReq);

            //Assert.IsNotNull(container);
            //Assert.AreEqual(containerName, container.Name);
            //Assert.AreEqual(12345, container.TotalBytesUsed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageContainerWithBadAuth()
        {
            var containerName = "TestContainer";

            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());
            
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageContainer(containerReq);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageContainerHasInternalServerError()
        {
            var containerName = "TestContainer";

            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.CreateStorageContainer(containerReq);
        }

        #endregion

        #region Delete Storage Container Tests

        [TestMethod]
        public async Task CanDeleteStorageContainerWithNoContentResponse()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageContainer(containerName);
        }

        [TestMethod]
        public async Task CanDeleteStorageContainerWithOkResponse()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageContainerWithObjects()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Conflict);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageContainerWithBadAuth()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageContainerWithInternalServerError()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageContainer(containerName);
        }

        #endregion

        #region Delete Storage Object Tests

        [TestMethod]
        public async Task CanDeleteStorageObjectWithNoContentResponse()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageObject(containerName, objectName);
        }

        [TestMethod]
        public async Task CanDeleteStorageObjectWithOkResponse()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageObjectWithBadAuth()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageObjectWithInternalServerError()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageObject(containerName, objectName);
        }

        #endregion

        #region Delete Storage Folder Tests

        [TestMethod]
        public async Task CanDeleteStorageFolderWithNoContentResponse()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageFolder(containerName, folderName);
        }

        [TestMethod]
        public async Task CanDeleteStorageFolderWithOkResponse()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageFolderThatHasChildren()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";
            var headers = new HttpHeadersAbstraction()
            {
                {"X-Container-Bytes-Used", "1234"},
                {"X-Container-Object-Count", "1"}
            };

            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, headers, HttpStatusCode.OK);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageFolderWithBadAuth()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageFolderWithInternalServerError()
        {
            var containerName = "TestContainer";
            var folderName = "a/b/c/";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.DeleteStorageFolder(containerName, folderName);
        }

        #endregion

        #region Update Storage Container Tests

        [TestMethod]
        public async Task CanUpdateAStorageContainerWithNoContentResponse()
        {
            var containerName = "TestContainer";
            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateStorageContainer(containerReq);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenUpdatingAStorageContainerWithBadAuth()
        {
            var containerName = "TestContainer";
            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateStorageContainer(containerReq);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenUpdatingAStorageContainerWithInternalServerError()
        {
            var containerName = "TestContainer";
            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateStorageContainer(containerReq);
        }

        #endregion

        #region Update Storage Object Tests

        [TestMethod]
        public async Task CanUpdateAStorageObjectWithAcceptedResponse()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objectReq = new StorageObject(containerName, objectName);

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Accepted);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateStorageObject(objectReq);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenUpdatingAStorageObjectWithBadAuth()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objectReq = new StorageObject(containerName, objectName);

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateStorageObject(objectReq);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenUpdatingAStorageObjectWithInternalServerError()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objectReq = new StorageObject(containerName, objectName);

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Responses.Enqueue(restResp);

            var client = new StorageServicePocoClient(GetValidContext(), this.ServiceLocator);
            await client.UpdateStorageObject(objectReq);
        }

        #endregion
    }
}
