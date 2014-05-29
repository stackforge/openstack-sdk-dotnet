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
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class StorageServiceClientTests
    {
        internal TestStorageServicePocoClient ServicePocoClient;
        internal TestLargeStorageObjectCreator loCreator;
        
        internal string authId = "12345";
        internal string endpoint = "http://teststorageendpoint.com/v1/1234567890";
        internal ServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.ServicePocoClient = new TestStorageServicePocoClient();
            this.loCreator = new TestLargeStorageObjectCreator();
            this.ServiceLocator = new ServiceLocator();
            
            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IStorageServicePocoClientFactory), new TestStorageServicePocoClientFactory(this.ServicePocoClient));
            manager.RegisterServiceInstance(typeof(ILargeStorageObjectCreatorFactory), new TestLargeStorageObjectCreatorFactory(this.loCreator));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.ServicePocoClient = new TestStorageServicePocoClient();
            this.loCreator = new TestLargeStorageObjectCreator();
            this.ServiceLocator = new ServiceLocator();
        }

        IOpenStackCredential GetValidCreds()
        {
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Swift", "Storage Service",
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
        public async Task CanListStorageObjects()
        {
            var containerName = "TestContainer";
            var numberObjCalls = 0;
            var obj = new StorageObject("TestObj", containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            var container = new StorageContainer(containerName, 100, 1, new Dictionary<string, string>(),
                new List<StorageObject>() {obj});
            this.ServicePocoClient.GetStorageContainerDelegate = s =>
            {
                Assert.AreEqual(container.Name, s);
                return Task.Factory.StartNew(() => container);
            };
            this.ServicePocoClient.GetStorageObjectDelegate = (s, s1) =>
            {
                numberObjCalls++;
                Assert.AreEqual(s, obj.ContainerName);
                Assert.AreEqual(s1, obj.Name);
                return Task.Factory.StartNew(() => obj);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.ListStorageObjects(containerName);

            Assert.AreEqual(1,numberObjCalls);
        }

        [TestMethod]
        public async Task CanListStorageObjectsWith404()
        {
            var containerName = "TestContainer";
            var obj = new StorageObject("TestObj", containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            var container = new StorageContainer(containerName, 100, 1, new Dictionary<string, string>(),
                new List<StorageObject>() { obj });
            this.ServicePocoClient.GetStorageContainerDelegate = s =>
            {
                Assert.AreEqual(container.Name, s);
                return Task.Factory.StartNew(() => container);
            };
            this.ServicePocoClient.GetStorageObjectDelegate = (s, s1) =>
            {
                throw new InvalidOperationException("Cannot get storage object. '" +HttpStatusCode.NotFound +"'");
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.ListStorageObjects(containerName);

            Assert.AreEqual(0, resp.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ListingStorageObjectsWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.ListStorageObjects(null);
        }

        [TestMethod]
        public async Task CanListStorageContainers()
        {
            var containerName = "TestContainer";
            var numberContainerCalls = 0;

            var container = new StorageContainer(containerName, 100, 1, new Dictionary<string, string>(),
                new List<StorageObject>());

            var account = new StorageAccount("1234567890", 100, 1, 1, new List<StorageContainer>() { container });

            this.ServicePocoClient.GetStorageContainerDelegate = s =>
            {
                numberContainerCalls++;
                Assert.AreEqual(container.Name, s);
                return Task.Factory.StartNew(() => container);
            };

            this.ServicePocoClient.GetStorageAccountDelegate = () => Task.Factory.StartNew(() => account);

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.ListStorageContainers();
            var containers = resp.ToList();

            Assert.AreEqual(1, containers.Count);
            Assert.AreEqual(1, numberContainerCalls);
        }

        [TestMethod]
        public async Task CanGetStorageAccount()
        {
            var account = new StorageAccount("1234567890", 100, 10, 1, new List<StorageContainer>());

            this.ServicePocoClient.GetStorageAccountDelegate = () =>
            {
                Assert.AreEqual("1234567890", account.Name);
                return Task.Factory.StartNew(() => account);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetStorageAccount();

            Assert.AreEqual(account, resp);
        }

        [TestMethod]
        public async Task CanGetStorageObjects()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            this.ServicePocoClient.GetStorageObjectDelegate = (s, s1) =>
            {
                Assert.AreEqual(s, obj.ContainerName);
                Assert.AreEqual(s1, obj.Name);
                return Task.Factory.StartNew(() => obj);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetStorageObject(containerName, objectName);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageObjectsWithNullContainerNameThrows()
        {
            var objectName = "TestObject";

           var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
           await client.GetStorageObject(null, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingStorageObjectsWithEmptyContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageObject(string.Empty, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageObjectsWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageObject(containerName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingStorageObjectsWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
           await client.GetStorageObject(containerName, string.Empty);
        }

        [TestMethod]
        public async Task CanGetStorageManifest()
        {
            var containerName = "TestContainer";
            var manifestName = "TestManifest";

            var obj = new StaticLargeObjectManifest(manifestName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            this.ServicePocoClient.GetStorageManifestDelegate = (s, s1) =>
            {
                Assert.AreEqual(s, obj.ContainerName);
                Assert.AreEqual(s1, obj.FullName);
                return Task.Factory.StartNew(() => (StorageManifest)obj);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetStorageManifest(containerName, manifestName);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageManifestWithNullContainerNameThrows()
        {
            var manifestName = "TestManifest";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageManifest(null, manifestName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingStorageManifestWithEmptyContainerNameThrows()
        {
            var manifestName = "TestManifest";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageObject(string.Empty, manifestName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageManifestWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageManifest(containerName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingStorageManifestWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageManifest(containerName, string.Empty);
        }

        [TestMethod]
        public async Task CanGetStorageFolder()
        {
            var containerName = "TestContainer";
            var folderName = "TestFolder/";

            var obj = new StorageFolder(folderName, new List<StorageFolder>());

            this.ServicePocoClient.GetStorageFolderDelegate = (s, s1) =>
            {
                Assert.AreEqual(s, containerName);
                Assert.AreEqual(s1, folderName);
                return Task.Factory.StartNew(() => obj);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetStorageFolder(containerName, folderName);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        public async Task CanGetStorageFolderWithoutTrailingSlash()
        {
            var containerName = "TestContainer";
            var folderName = "TestFolder";

            var obj = new StorageFolder(folderName, new List<StorageFolder>());

            this.ServicePocoClient.GetStorageFolderDelegate = (s, s1) =>
            {
                Assert.AreEqual(s, containerName);
                Assert.AreEqual(s1, folderName +"/");
                return Task.Factory.StartNew(() => obj);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetStorageFolder(containerName, folderName);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageFolderWithNullContainerNameThrows()
        {
            var folderName = "TestFolder";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageFolder(null, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingStorageFolderWithEmptyContainerNameThrows()
        {
            var folderName = "TestFolder";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageFolder(string.Empty, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageFolderWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageFolder(containerName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingStorageFolderWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageFolder(containerName, string.Empty);
        }

        [TestMethod]
        public async Task CanCreateStorageObjects()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var content = TestHelper.CreateStream("Some Data");

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            this.ServicePocoClient.CreateStorageObjectDelegate = async (s, stream) =>
            {
                Assert.AreEqual(s.ContainerName, obj.ContainerName);
                Assert.AreEqual(s.Name, obj.Name);
                Assert.AreEqual(stream, content);
                return await Task.Run(()=>obj);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.CreateStorageObject(containerName, objectName, new Dictionary<string,string>(), content);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        public async Task CreatingAnObjectLargerThanTheThresholdCreatesObjectWithSegments()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);

            var containerName = "TestContainer";
            var objectName = "TestObject";
            var content = "This is a lot of text that is bigger then the threshold that I set.".ConvertToStream();
            var metadata = new Dictionary<string, string>();

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            this.loCreator.CreateDelegate = async (c, o, m, s, n, sc) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                Assert.AreEqual(metadata, m);
                Assert.AreEqual(s, content);
                Assert.AreEqual(client.LargeObjectSegments, n);
                Assert.AreEqual(client.LargeObjectSegmentContainer, sc);
                return await Task.Run(() => obj);
            };

            
            client.LargeObjectThreshold = 10;

            var resp = await client.CreateStorageObject(containerName, objectName, metadata, content);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        public async Task CanCreateLargeObject()
        {
            var segmentsContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();


            this.loCreator.CreateDelegate = async (c, o, m, s, n, sc) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                Assert.AreEqual(metadata, m);
                Assert.AreEqual(s, contentStream);
                Assert.AreEqual(3, n);
                Assert.AreEqual(segmentsContainer, sc);
                return await Task.Run(() => new StorageObject(o,c));
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(containerName, objectName, metadata, contentStream, 3);

            Assert.AreEqual(containerName, res.ContainerName);
            Assert.AreEqual(objectName, res.FullName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullContainerName()
        {
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(null, objectName, metadata, contentStream, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithEmptyContainerName()
        {
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(string.Empty, objectName, metadata, contentStream, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullObjectName()
        {
            var containerName = "TestContainer";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(containerName, null, metadata, contentStream, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithEmptyObjectName()
        {
            var containerName = "TestContainer";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(containerName, string.Empty, metadata, contentStream, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullMetadata()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(containerName, objectName, null, contentStream, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullContent()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(containerName, objectName, metadata, null, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithNegativeSegmentCount()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(containerName, objectName, metadata, contentStream, -3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithZeroSegmentCount()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var res = await client.CreateLargeStorageObject(containerName, objectName, metadata, contentStream, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageObjectsWithNullContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageObject(null, objectName, new Dictionary<string, string>(), new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageObjectsWithEmptyContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageObject(string.Empty, objectName, new Dictionary<string, string>(), new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageObjectsWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageObject(containerName, null, new Dictionary<string, string>(), new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageObjectsWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageObject(containerName, string.Empty, new Dictionary<string, string>(), new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageObjectsWithNullStreamThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageObject(containerName, objectName, new Dictionary<string, string>(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageObjectsWithNullMetadataThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageObject(containerName, objectName, null, new MemoryStream());
        }

        [TestMethod]
        public async Task CanCreateStaticStorageManifest()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            this.ServicePocoClient.CreateStorageManifestDelegate = async (m) =>
            {
                Assert.IsInstanceOfType(m,typeof(StaticLargeObjectManifest));
                return await Task.Run(() => m);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, objectName, new Dictionary<string, string>(), new List<StorageObject>() { obj });
        }

        [TestMethod]
        public async Task CanCreateDynamicStorageManifest()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

           this.ServicePocoClient.CreateStorageManifestDelegate = async (m) =>
            {
                Assert.IsInstanceOfType(m, typeof(DynamicLargeObjectManifest));
                return await Task.Run(() => m);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, objectName, new Dictionary<string, string>(), "segments");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingDynamicStorageManifestWithNullContainerNameThrows()
        {
            var manifestName = "TestManifest";
            var segmentPath = "segments";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(null, manifestName, new Dictionary<string, string>(), segmentPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingDynamicStorageManifestWithEmptyContainerNameThrows()
        {
            var manifestName = "TestManifest";
            var segmentPath = "segments";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(string.Empty, manifestName, new Dictionary<string, string>(), segmentPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingDynamicStorageManifestWithNullManifestNameThrows()
        {
            var containerName = "TestContainer";
            var segmentPath = "segments";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, null, new Dictionary<string, string>(), segmentPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingDynamicStorageManifestWithEmptyManifestNameThrows()
        {
            var containerName = "TestContainer";
            var segmentPath = "segments";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, string.Empty, new Dictionary<string, string>(), segmentPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingDynamicStorageManifestWithNullSegmentPathThrows()
        {
            var containerName = "TestContainer";
            var manifestName = "TestManifest";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, manifestName, new Dictionary<string, string>(), (string)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingDynamicStorageManifestWithEmptySegmentPathThrows()
        {
            var containerName = "TestContainer";
            var manifestName = "TestManifest";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, manifestName, new Dictionary<string, string>(), string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingDynamicStorageManifestWithNullMetadataThrows()
        {
            var containerName = "TestContainer";
            var manifestName = "TestManifest";
            var segmentPath = "segments";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, manifestName, null, segmentPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStaticStorageManifestWithNullContainerNameThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(null, objectName, new Dictionary<string, string>(), new List<StorageObject>() { obj });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStaticStorageManifestWithEmptyContainerNameThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(string.Empty, objectName, new Dictionary<string, string>(), new List<StorageObject>() { obj });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStaticStorageManifestWithNullManifestNameThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, null, new Dictionary<string, string>(), new List<StorageObject>() { obj });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStaticStorageManifestWithEmptyManifestNameThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, string.Empty, new Dictionary<string, string>(), new List<StorageObject>() { obj });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStaticStorageManifestWithNullObjectListThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, objectName, new Dictionary<string, string>(), (List<StorageObject>)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStaticStorageManifestWithNullMetadataThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageManifest(containerName, objectName, null, new List<StorageObject>() { obj });
        }

        [TestMethod]
        public async Task CanCreateStorageFolder()
        {
            var containerName = "TestContainer";
            var folderName = "TestFolder/";

            var obj = new StorageFolder(folderName, new List<StorageFolder>());

            this.ServicePocoClient.CreateStorageFolderDelegate = async (s, s1) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(s1, folderName);
                    Assert.AreEqual(s, containerName);
                });
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        [TestMethod]
        public async Task CanCreateStorageFolderWithoutTrailingSlash()
        {
            var containerName = "TestContainer";
            var folderName = "TestFolder";

            var obj = new StorageFolder(folderName, new List<StorageFolder>());

            this.ServicePocoClient.CreateStorageFolderDelegate = async (s, s1) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(s1, folderName +"/");
                    Assert.AreEqual(s, containerName);
                });
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageFolderWithInvalidFolderNameThrows()
        {
            var containerName = "someContainer";
            var folderName = "Test//Folder";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageFolderWithNullContainerNameThrows()
        {
            var folderName = "TestFolder";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageFolder(null, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageFolderWithEmptyContainerNameThrows()
        {
            var folderName = "TestFolder";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageFolder(string.Empty, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageFolderWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageFolder(containerName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageFolderWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageFolder(containerName, string.Empty);
        }

        [TestMethod]
        public async Task CanCreateStorageContainers()
        {
            var containerName = "TestContainer";

            var obj = new StorageContainer(containerName, new Dictionary<string, string>());

            this.ServicePocoClient.CreateStorageContainerDelegate = async (s) =>
            {
                Assert.AreEqual(s.Name, obj.Name);
                 return await Task.Run(()=>obj);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageContainer(containerName, new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageContainersWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageContainer(null, new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageContainersWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageContainer(string.Empty, new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageContainersWithNullMetadataThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.CreateStorageContainer("TestContainer", null);
        }

        [TestMethod]
        public async Task CanGetStorageContainer()
        {
            var containerName = "TestContainer";

            var obj = new StorageContainer(containerName, new Dictionary<string, string>());

            this.ServicePocoClient.GetStorageContainerDelegate = (s) =>
            {
                Assert.AreEqual(s, obj.Name);
                return Task.Factory.StartNew(() => obj);
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetStorageContainer(containerName);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageContainersWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageContainer(null);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public async Task GettingStorageContainersWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.GetStorageContainer(string.Empty);
        }

        [TestMethod]
        public async Task CanUpdateStorageContainer()
        {
            var containerName = "TestContainer";

            var obj = new StorageContainer(containerName, new Dictionary<string, string>());

            this.ServicePocoClient.UpdateStorageContainerDelegate = async (s) =>
            {
                await Task.Run(() => Assert.AreEqual(s.Name, obj.Name));
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.UpdateStorageContainer(obj);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatingStorageContainersWithNullContainerThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.UpdateStorageContainer(null);
        }

        [TestMethod]
        public async Task CanDeleteStorageContainer()
        {
            var containerName = "TestContainer";

            var obj = new StorageContainer(containerName, new Dictionary<string, string>());

            this.ServicePocoClient.DeleteStorageConainerDelegate = async (s) =>
            {
                await Task.Run(()=>Assert.AreEqual(s, obj.Name));
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageContainer(obj.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletingStorageContainersWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageContainer(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeletingStorageContainersWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageContainer(string.Empty);
        }

        [TestMethod]
        public async Task CanDeleteStorageObject()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            this.ServicePocoClient.DeleteStorageObjectDelegate = async (s, s1) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(s, obj.ContainerName);
                    Assert.AreEqual(s1, obj.Name);
                });
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageObject(obj.ContainerName,obj.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletingStorageObjectWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageObject(null,"TestObject");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeletingStorageObjectsWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageObject(string.Empty, "TestObject");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletingStorageObjectWithNullObjectNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageObject("TestContainer", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeletingStorageObjectsWithEmptyObjectNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageObject("TestContainer", string.Empty);
        }

        [TestMethod]
        public async Task CanDeleteStorageFolder()
        {
            var containerName = "TestContainer";
            var folderName = "TestFolder/";

            this.ServicePocoClient.DeleteStorageFolderDelegate = async (s, s1) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(s, containerName);
                    Assert.AreEqual(s1, folderName);
                });
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageFolder(containerName, folderName);
        }

        [TestMethod]
        public async Task CanDeleteStorageFolderWithoutTrailingSlash()
        {
            var containerName = "TestContainer";
            var folderName = "TestFolder";

            this.ServicePocoClient.DeleteStorageFolderDelegate = async (s, s1) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(s, containerName);
                    Assert.AreEqual(s1, folderName +"/");
                });
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageFolder(containerName, folderName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletingStorageFolderWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageFolder(null, "TestFolder");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeletingStorageFolderWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageFolder(string.Empty, "TestFolder");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletingStorageFolderWithNullObjectNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageFolder("TestContainer", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeletingStorageFolderWithEmptyObjectNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DeleteStorageFolder("TestContainer", string.Empty);
        }

        [TestMethod]
        public async Task CanUpdateStorageObject()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
               "application/octet-stream", new Dictionary<string, string>());

            this.ServicePocoClient.UpdateStorageObjectDelegate = async (s) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(s.ContainerName, obj.ContainerName);
                    Assert.AreEqual(s.Name, obj.Name);
                });
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.UpdateStorageObject(obj);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatingStorageObjectWithNullContainerThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.UpdateStorageObject(null);
        }

        [TestMethod]
        public async Task CanDownloadStorageObjects()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var data = "some data";
            var content = TestHelper.CreateStream(data);
            var respStream = new MemoryStream();

            var obj = new StorageObject(objectName, containerName, DateTime.UtcNow, "12345", 12345,
                "application/octet-stream", new Dictionary<string, string>());

            this.ServicePocoClient.DownloadStorageObjectDelegate = async (s, s1, stream) =>
            {
                Assert.AreEqual(s, obj.ContainerName);
                Assert.AreEqual(s1, obj.Name);
                
                await content.CopyToAsync(stream);

                return obj;
            };

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            var resp = await client.DownloadStorageObject(containerName, objectName, respStream);
            respStream.Position = 0;

            Assert.AreEqual(obj, resp);
            Assert.AreEqual(data,TestHelper.GetStringFromStream(respStream));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DownloadingStorageObjectsWithNullContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DownloadStorageObject(null, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DownloadingStorageObjectsWithEmptyContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DownloadStorageObject(string.Empty, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DownloadingStorageObjectsWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DownloadStorageObject(containerName, null, new MemoryStream() );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DownloadingStorageObjectsWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DownloadStorageObject(containerName, string.Empty, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task DownloadingStorageObjectsWithNullStreamThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), "Swift", CancellationToken.None, this.ServiceLocator);
            await client.DownloadStorageObject(containerName, objectName, null);
        }
    }
}
