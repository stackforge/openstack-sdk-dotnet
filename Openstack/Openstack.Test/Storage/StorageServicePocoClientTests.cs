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
using Openstack.Common;
using Openstack.Common.Http;
using Openstack.Common.ServiceLocation;
using Openstack.Identity;
using Openstack.Storage;

namespace Openstack.Test.Storage
{
    [TestClass]
    public class StorageServicePocoClientTests
    {
        internal TestOpenstackServiceEndpointResolver resolver;
        internal TestStorageServiceRestClient StorageServiceRestClient;
        internal string authId = "12345";
        internal Uri endpoint = new Uri("http://teststorageendpoint.com/v1/1234567890");

        [TestInitialize]
        public void TestSetup()
        {
            this.StorageServiceRestClient = new TestStorageServiceRestClient();
            this.resolver = new TestOpenstackServiceEndpointResolver() { Endpoint = endpoint };

            ServiceLocator.Reset();
            var manager = ServiceLocator.Instance.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IStorageServiceRestClientFactory), new TestStorageServiceRestClientFactory(StorageServiceRestClient));
            manager.RegisterServiceInstance(typeof(IOpenstackServiceEndpointResolver), this.resolver);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.resolver = new TestOpenstackServiceEndpointResolver() { Endpoint = endpoint };
            this.StorageServiceRestClient = new TestStorageServiceRestClient();
            ServiceLocator.Reset();
        }

        StorageServiceClientContext GetValidContext()
        {
            var creds = new OpenstackCredential(this.endpoint, "SomeUser", "Password".ConvertToSecureString(), "SomeTenant", "region-a.geo-1");
            creds.SetAccessTokenId(this.authId);

            return new StorageServiceClientContext(creds, CancellationToken.None, "Object Storage");
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClientFactory().Create(GetValidContext()) as StorageServicePocoClient;
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageContainerAndNotAuthed()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageContainerAndServerError()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageContainerWithNullName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageAccount();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageAccountAndServerError()
        {
            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());

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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DownloadStorageObject(containerName, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDownloadingAStorageObjectAndNotAuthed()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DownloadStorageObject(containerName, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenDownloadingAStorageObjectAndServerError()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DownloadStorageObject(containerName, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotDownloadStorageObjectWithNullContainerName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
            await client.DownloadStorageObject(null, "object", new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotDownloadStorageObjectWithNullObjectName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
            await client.DownloadStorageObject("container", null, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotDownloadStorageObjectWithEmptyContainerName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
            await client.DownloadStorageObject(string.Empty, "object", new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotDownloadStorageObjectWithEmptyObjectName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
            await client.DownloadStorageObject("container", string.Empty, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotDownloadStorageObjectWithnullOutputStream()
        {
            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageObjectAndNotAuthed()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionthrownWhenGettingAStorageObjectAndServerError()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageObjectWithNullContainerName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageObject(null,"object");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotGetStorageObjectWithNullObjectName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageObject("container", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetStorageObjectWithEmptyContainerName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageObject(string.Empty, "object");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotGetStorageObjectWithEmptyObjectName()
        {
            var client = new StorageServicePocoClient(GetValidContext());
            await client.GetStorageObject("container", string.Empty);
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageObjectMissingLength()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var objRequest = new StorageObject(objectName, containerName);
            var content = TestHelper.CreateStream("Some Content");

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.LengthRequired);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.CreateStorageObject(objRequest, content);
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
            this.StorageServiceRestClient.Response = restResp;

            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.CreateStorageContainer(containerReq);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenCreatingaStorageContainerHasInternalServerError()
        {
            var containerName = "TestContainer";

            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.CreateStorageContainer(containerReq);
        }

        #endregion

        #region Delete Storage Container Tests

        [TestMethod]
        public async Task CanDeleteStorageContainerWithNoContentResponse()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DeleteStorageContainer(containerName);
        }

        [TestMethod]
        public async Task CanDeleteStorageContainerWithOkResponse()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DeleteStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageContainerWithObjects()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Conflict);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DeleteStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageContainerWithBadAuth()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DeleteStorageContainer(containerName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageContainerWithInternalServerError()
        {
            var containerName = "TestContainer";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DeleteStorageObject(containerName, objectName);
        }

        [TestMethod]
        public async Task CanDeleteStorageObjectWithOkResponse()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DeleteStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageObjectWithBadAuth()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DeleteStorageObject(containerName, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenDeletingAStorageObjectWithInternalServerError()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.DeleteStorageObject(containerName, objectName);
        }

        #endregion

        #region Update Storage Container Tests

        [TestMethod]
        public async Task CanUpdateAStorageContainerWithNoContentResponse()
        {
            var containerName = "TestContainer";
            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NoContent);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.UpdateStorageContainer(containerReq);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenUpdatingAStorageContainerWithBadAuth()
        {
            var containerName = "TestContainer";
            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.UpdateStorageContainer(containerReq);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ExceptionThrownWhenUpdatingAStorageContainerWithInternalServerError()
        {
            var containerName = "TestContainer";
            var containerReq = new StorageContainer(containerName, new Dictionary<string, string>());

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
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
            this.StorageServiceRestClient.Response = restResp;

            var client = new StorageServicePocoClient(GetValidContext());
            await client.UpdateStorageObject(objectReq);
        }

        #endregion
    }
}
