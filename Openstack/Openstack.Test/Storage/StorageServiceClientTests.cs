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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Openstack.Common;
using Openstack.Common.ServiceLocation;
using Openstack.Identity;
using Openstack.Storage;

namespace Openstack.Test.Storage
{
    [TestClass]
    public class StorageServiceClientTests
    {
        internal TestStorageServicePocoClient ServicePocoClient;
        internal string authId = "12345";
        internal string endpoint = "http://teststorageendpoint.com/v1/1234567890";

        [TestInitialize]
        public void TestSetup()
        {
            this.ServicePocoClient = new TestStorageServicePocoClient();

            ServiceLocator.Reset();
            var manager = ServiceLocator.Instance.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IStorageServicePocoClientFactory), new TestStorageServicePocoClientFactory(ServicePocoClient));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.ServicePocoClient = new TestStorageServicePocoClient();
            ServiceLocator.Reset();
        }

        IOpenstackCredential GetValidCreds()
        {
            var creds = new OpenstackCredential(new Uri(this.endpoint), "SomeUser", "Password".ConvertToSecureString(), "SomeTenant");
            creds.SetAccessTokenId(this.authId);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.ListStorageObjects(containerName);

            Assert.AreEqual(1,numberObjCalls);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ListingStorageObjectsWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            var resp = await client.GetStorageObject(containerName, objectName);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageObjectsWithNullContainerNameThrows()
        {
            var objectName = "TestObject";

           var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
           await client.GetStorageObject(null, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingStorageObjectsWithEmptyContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.GetStorageObject(string.Empty, objectName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageObjectsWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.GetStorageObject(containerName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GettingStorageObjectsWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
           await client.GetStorageObject(containerName, string.Empty);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            var resp = await client.CreateStorageObject(containerName, objectName, new Dictionary<string,string>(), content);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageObjectsWithNullContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageObject(null, objectName, new Dictionary<string, string>(), new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageObjectsWithEmptyContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageObject(string.Empty, objectName, new Dictionary<string, string>(), new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageObjectsWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageObject(containerName, null, new Dictionary<string, string>(), new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageObjectsWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageObject(containerName, string.Empty, new Dictionary<string, string>(), new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageObjectsWithNullStreamThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageObject(containerName, objectName, new Dictionary<string, string>(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageObjectsWithNullMetadataThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageObject(containerName, objectName, null, new MemoryStream());
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageContainer(containerName, new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageContainersWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageContainer(null, new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatingStorageContainersWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.CreateStorageContainer(string.Empty, new Dictionary<string, string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreatingStorageContainersWithNullMetadataThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            var resp = await client.GetStorageContainer(containerName);

            Assert.AreEqual(obj, resp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GettingStorageContainersWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.GetStorageContainer(null);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public async Task GettingStorageContainersWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.UpdateStorageContainer(obj);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatingStorageContainersWithNullContainerThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DeleteStorageContainer(obj.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletingStorageContainersWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DeleteStorageContainer(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeletingStorageContainersWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DeleteStorageObject(obj.ContainerName,obj.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletingStorageObjectWithNullContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DeleteStorageObject(null,"TestObject");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeletingStorageObjectsWithEmptyContainerNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DeleteStorageObject(string.Empty, "TestObject");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeletingStorageObjectWithNullObjectNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DeleteStorageObject("TestContainer", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeletingStorageObjectsWithEmptyObjectNameThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DeleteStorageObject("TestContainer", string.Empty);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.UpdateStorageObject(obj);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdatingStorageObjectWithNullContainerThrows()
        {
            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
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

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DownloadStorageObject(null, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DownloadingStorageObjectsWithEmptyContainerNameThrows()
        {
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DownloadStorageObject(string.Empty, objectName, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DownloadingStorageObjectsWithNullObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DownloadStorageObject(containerName, null, new MemoryStream() );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DownloadingStorageObjectsWithEmptyObjectNameThrows()
        {
            var containerName = "TestContainer";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DownloadStorageObject(containerName, string.Empty, new MemoryStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DownloadingStorageObjectsWithNullStreamThrows()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var client = new StorageServiceClient(GetValidCreds(), CancellationToken.None);
            await client.DownloadStorageObject(containerName, objectName, null);
        }
    }
}
