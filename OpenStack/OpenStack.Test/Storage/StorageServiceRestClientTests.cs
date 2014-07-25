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
using System.Security.Cryptography;
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
    public class StorageServiceRestClientTests
    {
        internal StorageRestSimulator simulator;
        internal string authId = "12345";
        internal Uri endpoint = new Uri("http://teststorageendpoint.com/v1/1234567890");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.simulator = new StorageRestSimulator();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IHttpAbstractionClientFactory), new StorageRestSimulatorFactory(simulator));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.simulator = new StorageRestSimulator();
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

            return new ServiceClientContext(creds, token, "Object Storage", endpoint);
        }

        #region CreateManifest Tests

        [TestMethod]
        public async Task CreateStaticStorageManifestIncludesAuthHeader()
        {
            var manifestName = "NewManifest";
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            await client.CreateStaticManifest(containerName, manifestName, new Dictionary<string, string>(), content);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task CreateDynamicStorageManifestIncludesAuthHeader()
        {
            var manifestName = "NewManifest";
            var segPath = "blah/blah";
            var containerName = "newContainer";

            var client = new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CreateDynamicManifest(containerName, manifestName, new Dictionary<string, string>(), segPath);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task CreateStaticManifestFormsCorrectUrlAndMethod()
        {
            var manifestName = "NewManifest";
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            await client.CreateStaticManifest(containerName, manifestName, new Dictionary<string, string>(), content);

            Assert.AreEqual(string.Format("{0}/{1}/{2}?multipart-manifest=put", endpoint, containerName, manifestName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Put, this.simulator.Method);
        }

        [TestMethod]
        public async Task CreateDynamicManifestFormsCorrectUrlMethodAndHeader()
        {
            var manifestName = "NewManifest";
            var segPath = "blah/blah";
            var containerName = "newContainer";

            var client = new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CreateDynamicManifest(containerName, manifestName, new Dictionary<string, string>(), segPath);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, containerName, manifestName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Put, this.simulator.Method);
            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Object-Manifest"));
            Assert.AreEqual(segPath, this.simulator.Headers["X-Object-Manifest"]);
        }

        #endregion

        #region CreateObject Tests

        [TestMethod]
        public async Task CreateStorageObjectIncludesAuthHeader()
        {

           var objectName = "NewObject";
            var containerName = "newContainer";


            var client = new StorageServiceRestClientFactory().Create(GetValidContext(), this.ServiceLocator) as StorageServiceRestClient;
           
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task CreateStorageObjectFormsCorrectUrlAndMethod()
        {
            var objectName = "NewObject";
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, containerName, objectName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Put, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanCreateStorageObject()
        {
            var objectName = "NewObject";
            var containerName = "newContainer";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            var resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));

            var objectContent = TestHelper.GetStringFromStream(this.simulator.Objects[objectName].Content);
            Assert.AreEqual(data,objectContent);
        }

        [TestMethod]
        public async Task CreatingStorageObjectMultipleTimesOverwrites()
        {
            var containerName = "newContainer";
            var objectName = "NewObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            var resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));
            
            var otherData = "Other data that is longer";
            var otherContent = TestHelper.CreateStream(otherData);

            resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), otherContent);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));

            var objectContent = TestHelper.GetStringFromStream(this.simulator.Objects[objectName].Content);
            Assert.AreEqual(otherData, objectContent);
        }

        [TestMethod]
        public async Task CreatedStorageObjectHasETag()
        {
            var objectName = "NewObject";
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);
            var hash = Convert.ToBase64String(MD5.Create().ComputeHash(content));
            content.Position = 0;

            var resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "ETag"));
            Assert.AreEqual(hash, resp.Headers.First(kvp => kvp.Key == "ETag").Value.First());

            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));

            var objectContent = TestHelper.GetStringFromStream(this.simulator.Objects[objectName].Content);
            Assert.AreEqual(data, objectContent);
        }

        [TestMethod]
        public async Task CanCreateStorageObjectWithMetaData()
        {
            var objectName = "NewObject";
            var containerName = "newContainer";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            var metaData = new Dictionary<string, string> {{"Test1", "Test1"}, {"Test2", "Test2"}};

            var resp = await client.CreateObject(containerName, objectName, metaData, content);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Object-Meta-Test1"));
            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Object-Meta-Test2"));
            Assert.AreEqual("Test1", this.simulator.Headers["X-Object-Meta-Test1"]);
            Assert.AreEqual("Test2", this.simulator.Headers["X-Object-Meta-Test2"]);

            var objectContent = TestHelper.GetStringFromStream(this.simulator.Objects[objectName].Content);
            Assert.AreEqual(data, objectContent);
        }

        [TestMethod]
        public async Task CreateStorageObjectWithTrailingForwardSlashesInName()
        {
            var objectName = "NewObject/////";
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            var resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, containerName, objectName), this.simulator.Uri.ToString());

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));

            var objectContent = TestHelper.GetStringFromStream(this.simulator.Objects[objectName].Content);
            Assert.AreEqual(data, objectContent);
        }

        [TestMethod]
        public async Task CreateStorageObjectWithLeadingForwardSlashesInName()
        {
            var objectName = "//NewObject";
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            var resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, containerName, objectName), this.simulator.Uri.ToString());

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));

            var objectContent = TestHelper.GetStringFromStream(this.simulator.Objects[objectName].Content);
            Assert.AreEqual(data, objectContent);
        }

        [TestMethod]
        public async Task CreateStorageObjectWithForwardSlashesInName()
        {
            var objectName = "New/Object";
            var containerName = "newContainer";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            var resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", this.endpoint, containerName, objectName), this.simulator.Uri.ToString());

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));

            var objectContent = TestHelper.GetStringFromStream(this.simulator.Objects[objectName].Content);
            Assert.AreEqual(data, objectContent);
        }

        [TestMethod]
        public async Task CreateStorageObjectWithNullContent()
        {
            var objectName = "NewObject/////";
            var containerName = "newContainer";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), null);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Objects.ContainsKey(objectName));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateStorageObjectWithSlashesInContainerName()
        {
            var objectName = "NewObject";
            var containerName = "new/Container";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), null);
        }

        [TestMethod]
        public async Task TryingToCreateAnObjectWithBadAuthenticationFails()
        {
            var authId = "54321";
            var objectName = "NewObject";
            var containerName = "newContainer";

            var context = GetValidContext();
            context.Credential.SetAccessTokenId(authId);
            var client =
                new StorageServiceRestClient(context, this.ServiceLocator);

            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            var resp = await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            Assert.IsFalse(this.simulator.Objects.ContainsKey(objectName));
        }

        [TestMethod]
        public async Task TryingToCreateAnObjectAndCancelThrowsException()
        {
            var objectName = "NewObject";
            var containerName = "newContainer";
            var token = new CancellationToken(true);
            this.simulator.Delay = TimeSpan.FromMilliseconds(500);

            var client =
                new StorageServiceRestClient(GetValidContext(token), this.ServiceLocator);
            var data = "Some random data";
            var content = TestHelper.CreateStream(data);

            try
            {
                await client.CreateObject(containerName, objectName, new Dictionary<string, string>(), content);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex,typeof(OperationCanceledException));
                Assert.IsFalse(this.simulator.Objects.ContainsKey(objectName));
            }
        }

        #endregion

        #region Create Container Tests

        [TestMethod]
        public async Task CreateStorageContainerIncludesAuthHeader()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CreateContainer(containerName, new Dictionary<string, string>());

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task CreateStorageContainerFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CreateContainer(containerName, new Dictionary<string, string>());

            Assert.AreEqual(string.Format("{0}/{1}", endpoint, containerName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Put, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanCreateStorageContainer()
        {
            var containerName = "newContainer";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CreateContainer(containerName, new Dictionary<string, string>());

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Containers.ContainsKey(containerName));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateStorageConainerWithSlashesInContainerName()
        {
            var containerName = "new/Container";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CreateContainer(containerName, new Dictionary<string, string>());
        }

        [TestMethod]
        public async Task CanCreateStorageContainerWithMetaData()
        {
            var containerName = "newContainer";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);
            
            var metaData = new Dictionary<string, string> { { "Test1", "Test1" }, { "Test2", "Test2" } };

            var resp = await client.CreateContainer(containerName, metaData);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Containers.ContainsKey(containerName));

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Container-Meta-Test1"));
            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Container-Meta-Test2"));
            Assert.AreEqual("Test1", this.simulator.Headers["X-Container-Meta-Test1"]);
            Assert.AreEqual("Test2", this.simulator.Headers["X-Container-Meta-Test2"]);
        }

        [TestMethod]
        public async Task CreatingStorageContainerMultipleTimesOverwrites()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CreateContainer(containerName, new Dictionary<string, string>());

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
            Assert.IsTrue(this.simulator.Containers.ContainsKey(containerName));

            var metaData = new Dictionary<string, string> { { "Test1", "Test1" }, { "Test2", "Test2" } };

            resp = await client.CreateContainer(containerName, metaData);

            Assert.AreEqual(HttpStatusCode.Accepted, resp.StatusCode);
            Assert.IsTrue(this.simulator.Containers.ContainsKey(containerName));
            Assert.IsTrue(this.simulator.Containers[containerName].MetaData.ContainsKey("X-Container-Meta-Test1"));
        }

        #endregion

        #region Get Container Tests

        [TestMethod]
        public async Task GetStorageContainerIncludesAuthHeader()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetContainer(containerName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetStorageContainerFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetContainer(containerName);

            Assert.AreEqual(string.Format("{0}/{1}", endpoint, containerName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenContainerIsNotFound()
        {
            var containerName = "newContainer";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetContainer(containerName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);

            //Add some assert here to validate that we got all the headers we expected... 
        }

        [TestMethod]
        public async Task CanGetStorageContainer()
        {
            var containerName = "newContainer";

            this.simulator.Containers.Add(containerName, new StorageRestSimulator.StorageItem(containerName));

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetContainer(containerName);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            //Add some assert here to validate that we got all the headers we expected... 
        }

        [TestMethod]
        public async Task CanGetStorageContainerWithMetadata()
        {
            var containerName = "newContainer";

            var metaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" }, { "X-Object-Meta-Test2", "Test2" } };

            this.simulator.Containers.Add(containerName, new StorageRestSimulator.StorageItem(containerName) { MetaData = metaData});

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetContainer(containerName);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);


            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Object-Meta-Test2"));
            Assert.AreEqual("Test2", resp.Headers.First(kvp => kvp.Key == "X-Object-Meta-Test2").Value.First());
        }

        #endregion

        #region Get Storage Object Tests

        [TestMethod]
        public async Task GetStorageObjectIncludesAuthHeader()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetObject(containerName, objectName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetStorageObjectFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetObject(containerName, objectName);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, containerName, objectName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenObjectIsNotFound()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetObject(containerName, objectName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetStorageObject()
        {
            var containerName = "newContainer";
            var objectName = "newObject";
            var data = "Some Data";

            var content = TestHelper.CreateStream(data);
            var hash = Convert.ToBase64String(MD5.Create().ComputeHash(content));
            content.Position = 0;

            this.simulator.Objects.Add(objectName, new StorageRestSimulator.StorageItem(objectName) { Content = content });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetObject(containerName, objectName);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.AreEqual(data, respContent);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "ETag"));
            Assert.AreEqual(hash, resp.Headers.First(kvp => kvp.Key == "ETag").Value.First());
        }

        [TestMethod]
        public async Task CanGetStorageObjectWithMetadata()
        {
            var containerName = "newContainer";
            var objectName = "newObject";
            var data = "Some Data";

            var content = TestHelper.CreateStream(data);

            var metaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" }, { "X-Object-Meta-Test2", "Test2" }};

            this.simulator.Objects.Add(objectName, new StorageRestSimulator.StorageItem(objectName) { MetaData = metaData, Content = content});

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetObject(containerName, objectName);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Object-Meta-Test2"));
            Assert.AreEqual("Test2", resp.Headers.First(kvp => kvp.Key == "X-Object-Meta-Test2").Value.First());

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.AreEqual(data,respContent);
        }

        #endregion

        #region Delete Storage Object Tests

        [TestMethod]
        public async Task DeleteStorageObjectIncludesAuthHeader()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.DeleteObject(containerName, objectName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task DeleteStorageObjectFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.DeleteObject(containerName, objectName);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, containerName, objectName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Delete, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenDeletingAnObjectThatIsNotFound()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.DeleteObject(containerName, objectName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanDeleteStorageObject()
        {
            var containerName = "newContainer";
            var objectName = "newObject";
            var data = "Some Data";

            var content = TestHelper.CreateStream(data);

            this.simulator.Objects.Add(objectName, new StorageRestSimulator.StorageItem(objectName) {Content = content});

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.DeleteObject(containerName, objectName);

            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);
            Assert.IsFalse(this.simulator.Objects.ContainsKey(objectName));
        }

        #endregion

        #region Delete Container Tests

        [TestMethod]
        public async Task DeleteStorageContainerIncludesAuthHeader()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.DeleteContainer(containerName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task DeleteStorageContainerFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.DeleteContainer(containerName);

            Assert.AreEqual(string.Format("{0}/{1}", endpoint, containerName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Delete, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenDeletingAContainerThatIsNotFound()
        {
            var containerName = "newContainer";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.DeleteContainer(containerName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenDeletingAContainerThatIsNotEmpty()
        {
            var containerName = "newContainer";

            this.simulator.Containers.Add(containerName, new StorageRestSimulator.StorageItem(containerName));
            this.simulator.IsContainerEmpty = false;

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.DeleteContainer(containerName);

            Assert.AreEqual(HttpStatusCode.Conflict, resp.StatusCode);
            Assert.IsTrue(this.simulator.Containers.ContainsKey(containerName));
        }

        [TestMethod]
        public async Task CanDeleteStorageContainer()
        {
            var containerName = "newContainer";

            this.simulator.Containers.Add(containerName, new StorageRestSimulator.StorageItem(containerName));

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.DeleteContainer(containerName);

            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);
            Assert.IsFalse(this.simulator.Containers.ContainsKey(containerName));
        }

        #endregion

        #region Update Storage Object Tests

        [TestMethod]
        public async Task UpdateStorageObjectIncludesAuthHeader()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var metadata = new Dictionary<string, string> { { "Test1", "Test1" }, { "Test2", "Test2" } };

            await client.UpdateObject(containerName, objectName, metadata);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task UpdateStorageObjectFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var metadata = new Dictionary<string, string> { { "Test1", "Test1" }, { "Test2", "Test2" } };

            await client.UpdateObject(containerName, objectName, metadata);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, containerName, objectName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Post, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenUpdatingAnObjectThatIsNotFound()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var metadata = new Dictionary<string, string> { { "Test1", "Test1" }, { "Test2", "Test2" } };

            var resp = await client.UpdateObject(containerName, objectName, metadata);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        public async Task CanUpdateAStorageObject()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var origMetaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" }};

            this.simulator.Objects.Add(objectName, new StorageRestSimulator.StorageItem(objectName) { MetaData = origMetaData });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var metadata = new Dictionary<string, string> { { "Test2", "Test2" } };

            var resp = await client.UpdateObject(containerName, objectName, metadata);

            Assert.AreEqual(HttpStatusCode.Accepted, resp.StatusCode);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Object-Meta-Test2"));
            Assert.IsFalse(resp.Headers.Any(kvp => kvp.Key == "X-Object-Meta-Test1"));
            Assert.AreEqual("Test2", resp.Headers.First(kvp => kvp.Key == "X-Object-Meta-Test2").Value.First());
        }

        #endregion

        #region Update Storage Container Tests

        [TestMethod]
        public async Task UpdateStorageContainerIncludesAuthHeader()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var metadata = new Dictionary<string, string> { { "Test1", "Test1" }, { "Test2", "Test2" } };

            await client.UpdateContainer(containerName, metadata);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task UpdateStorageContainerFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var metadata = new Dictionary<string, string> { { "Test1", "Test1" }, { "Test2", "Test2" } };

            await client.UpdateContainer(containerName, metadata);

            Assert.AreEqual(string.Format("{0}/{1}", endpoint, containerName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Post, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenUpdatingAContainerThatIsNotFound()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var metadata = new Dictionary<string, string> { { "Test1", "Test1" }, { "Test2", "Test2" } };

            var resp = await client.UpdateContainer(containerName, metadata);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanUpdateAStorageContainer()
        {
            var containerName = "newContainer";

            var origMetaData = new Dictionary<string, string> { { "X-Container-Meta-Test1", "Test1" } };

            this.simulator.Containers.Add(containerName, new StorageRestSimulator.StorageItem(containerName) { MetaData = origMetaData });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var metadata = new Dictionary<string, string> { { "Test2", "Test2" } };

            var resp = await client.UpdateContainer(containerName, metadata);
            Assert.AreEqual(HttpStatusCode.Accepted, resp.StatusCode);

            resp = await client.GetContainer(containerName);
            
            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Container-Meta-Test2"));
            Assert.IsFalse(resp.Headers.Any(kvp => kvp.Key == "X-Container-Meta-Test1"));
            Assert.AreEqual("Test2", resp.Headers.First(kvp => kvp.Key == "X-Container-Meta-Test2").Value.First());
        }

        #endregion

        #region Get Storage Container Metadata Tests

        [TestMethod]
        public async Task GetStorageContainerMetadataIncludesAuthHeader()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetContainerMetadata(containerName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetStorageContainerMetadataFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetContainerMetadata(containerName);

            Assert.AreEqual(string.Format("{0}/{1}", endpoint, containerName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Head, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenGettingMetadataForAContainerThatIsNotFound()
        {
            var containerName = "newContainer";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetContainerMetadata(containerName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetMetadataForAStorageContainer()
        {
            var containerName = "newContainer";

            var origMetaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" } };

            this.simulator.Containers.Add(containerName, new StorageRestSimulator.StorageItem(containerName) { MetaData = origMetaData });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetContainerMetadata(containerName);

            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Object-Meta-Test1"));
            Assert.AreEqual("Test1", resp.Headers.First(kvp => kvp.Key == "X-Object-Meta-Test1").Value.First());
        }

        #endregion

        #region Get Storage Object Metadata Tests

        [TestMethod]
        public async Task GetStorageObjectMetadataIncludesAuthHeader()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetObjectMetadata(containerName, objectName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetStorageObjectMetadataFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetObjectMetadata(containerName, objectName);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, containerName, objectName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Head, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenGettingMetadataForAnObjectThatIsNotFound()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetObjectMetadata(containerName, objectName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetMetadataForAStorageObject()
        {
            var containerName = "newContainer";
            var objectName = "newObject";

            var origMetaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" } };
            var content = TestHelper.CreateStream("Some Data");

            this.simulator.Objects.Add(objectName, new StorageRestSimulator.StorageItem(objectName) { MetaData = origMetaData, Content = content });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetObjectMetadata(containerName, objectName);

            Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Object-Meta-Test1"));
            Assert.AreEqual("Test1", resp.Headers.First(kvp => kvp.Key == "X-Object-Meta-Test1").Value.First());
        }

        #endregion

        #region Copy Storage Object Tests

        [TestMethod]
        public async Task CopyStorageObjectMetadataIncludesAuthHeader()
        {
            var sourceContainerName = "oldContainer";
            var sourceObjectName = "oldObject";
            var targetContainerName = "newContainer";
            var targetObjectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CopyObject(sourceContainerName, sourceObjectName, targetContainerName, targetObjectName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task CopyStorageObjectMetadataIncludesDestinationHeader()
        {
            var sourceContainerName = "oldContainer";
            var sourceObjectName = "oldObject";
            var targetContainerName = "newContainer";
            var targetObjectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CopyObject(sourceContainerName, sourceObjectName, targetContainerName, targetObjectName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("Destination"));
            Assert.AreEqual(string.Format("{0}/{1}", targetContainerName, targetObjectName), this.simulator.Headers["Destination"]);
        }

        [TestMethod]
        public async Task CopyStorageObjectMetadataFormsCorrectUrlAndMethod()
        {
            var sourceContainerName = "oldContainer";
            var sourceObjectName = "oldObject";
            var targetContainerName = "newContainer";
            var targetObjectName = "newObject";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.CopyObject(sourceContainerName, sourceObjectName, targetContainerName, targetObjectName);

            Assert.AreEqual(string.Format("{0}/{1}/{2}", endpoint, sourceContainerName, sourceObjectName), this.simulator.Uri.ToString());
            Assert.AreEqual(new HttpMethod("COPY"), this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenCopyingAnObjectAndSourceObjectIsNotFound()
        {
            var sourceContainerName = "oldContainer";
            var sourceObjectName = "oldObject";
            var targetContainerName = "newContainer";
            var targetObjectName = "newObject";

            this.simulator.Containers.Add(sourceContainerName, new StorageRestSimulator.StorageItem(sourceContainerName));
            this.simulator.Containers.Add(targetContainerName, new StorageRestSimulator.StorageItem(targetContainerName));

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CopyObject(sourceContainerName, sourceObjectName, targetContainerName, targetObjectName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenCopyingAnObjectAndSourceContainerIsNotFound()
        {
            var sourceContainerName = "oldContainer";
            var sourceObjectName = "oldObject";
            var targetContainerName = "newContainer";
            var targetObjectName = "newObject";

            var origMetaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" } };
            var content = TestHelper.CreateStream("Some Data");

            this.simulator.Containers.Add(targetContainerName, new StorageRestSimulator.StorageItem(targetContainerName));
            this.simulator.Objects.Add(sourceObjectName, new StorageRestSimulator.StorageItem(sourceObjectName) { MetaData = origMetaData, Content = content });

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CopyObject(sourceContainerName, sourceObjectName, targetContainerName, targetObjectName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenCopyingAnObjectAndDestinationContainerIsNotFound()
        {
            var sourceContainerName = "oldContainer";
            var sourceObjectName = "oldObject";
            var targetContainerName = "newContainer";
            var targetObjectName = "newObject";

            var origMetaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" } };
            var content = TestHelper.CreateStream("Some Data");

            this.simulator.Containers.Add(sourceContainerName, new StorageRestSimulator.StorageItem(sourceContainerName));
            this.simulator.Objects.Add(sourceObjectName, new StorageRestSimulator.StorageItem(sourceObjectName) { MetaData = origMetaData, Content = content });

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CopyObject(sourceContainerName, sourceObjectName, targetContainerName, targetObjectName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenTryingToCopyAContainer()
        {
            var sourceContainerName = "oldContainer";
            var sourceObjectName = "oldObject";
            var targetContainerName = "newContainer";
            var targetObjectName = "";

            var origMetaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" } };
            var content = TestHelper.CreateStream("Some Data");

            this.simulator.Containers.Add(sourceContainerName, new StorageRestSimulator.StorageItem(sourceContainerName));
            this.simulator.Objects.Add(sourceObjectName, new StorageRestSimulator.StorageItem(sourceObjectName) { MetaData = origMetaData, Content = content });

            var client = new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CopyObject(sourceContainerName, sourceObjectName, targetContainerName, targetObjectName);

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanCopyAStorageObject()
        {
            var sourceContainerName = "oldContainer";
            var sourceObjectName = "oldObject";
            var targetContainerName = "newContainer";
            var targetObjectName = "newObject";
            var data = "Some Data";

            var origMetaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" } };
            var content = TestHelper.CreateStream(data);

            this.simulator.Containers.Add(targetContainerName, new StorageRestSimulator.StorageItem(targetContainerName));
            this.simulator.Containers.Add(sourceContainerName, new StorageRestSimulator.StorageItem(sourceContainerName));
            this.simulator.Objects.Add(sourceObjectName, new StorageRestSimulator.StorageItem(sourceObjectName) { MetaData = origMetaData, Content = content });

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.CopyObject(sourceContainerName, sourceObjectName, targetContainerName, targetObjectName);

            Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);

            Assert.IsTrue(this.simulator.Objects.ContainsKey(targetObjectName));
            
            var obj = this.simulator.Objects[targetObjectName];
            Assert.AreEqual(data, TestHelper.GetStringFromStream(obj.Content));
            Assert.IsTrue(obj.MetaData.ContainsKey("X-Object-Meta-Test1"));
            Assert.AreEqual("Test1", obj.MetaData["X-Object-Meta-Test1"]);
        }

        #endregion

        #region Get AccountTests

        [TestMethod]
        public async Task GetStorageAccountIncludesAuthHeader()
        {
            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetAccount();

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetStorageAccountFormsCorrectUrlAndMethod()
        {
            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetAccount();

            Assert.AreEqual(string.Format("{0}", endpoint), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanGetAccount()
        {
            this.simulator.Containers.Add("TestContainer", new StorageRestSimulator.StorageItem("TestContainer"));

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetAccount();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Account-Container-Count"));
            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Account-Object-Count"));
            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Account-Bytes-Used"));
        }

        #endregion

        #region Get Storage Folder Tests

        [TestMethod]
        public async Task GetStorageFolderIncludesAuthHeader()
        {
            var containerName = "newContainer";
            var folderName = "newFolder";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFolder(containerName, folderName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetStorageFolderFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";
            var folderName = "a/b/b/";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFolder(containerName, folderName);

            Assert.AreEqual(string.Format("{0}/{1}?delimiter=/&prefix={2}", endpoint, containerName, folderName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task GetRootStorageFolderFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";
            var folderName = "/";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFolder(containerName, folderName);

            Assert.AreEqual(string.Format("{0}/{1}?delimiter=/", endpoint, containerName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenFolderIsNotFound()
        {
            var containerName = "newContainer";
            var folderName = "a/b/b/";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetFolder(containerName, folderName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetStorageFolder()
        {
            var containerName = "newContainer";
            var folderName = "a/b/b/";

            var content = TestHelper.CreateStream(string.Empty);
            content.Position = 0;

            this.simulator.Objects.Add(folderName, new StorageRestSimulator.StorageItem(folderName) { Content = content });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetFolder(containerName, folderName);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetStorageFolderWithMetadata()
        {
            var containerName = "newContainer";
            var folderName = "a/b/b/";

            var content = TestHelper.CreateStream(string.Empty);
            content.Position = 0;

            var metaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" }, { "X-Object-Meta-Test2", "Test2" } };

            this.simulator.Objects.Add(folderName, new StorageRestSimulator.StorageItem(folderName) { MetaData = metaData, Content = content });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetFolder(containerName, folderName);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Object-Meta-Test2"));
            Assert.AreEqual("Test2", resp.Headers.First(kvp => kvp.Key == "X-Object-Meta-Test2").Value.First());
        }

        #endregion

        #region Get Storage Manifest Tests

        [TestMethod]
        public async Task GetStorageManifestIncludesAuthHeader()
        {
            var containerName = "newContainer";
            var manifestName = "newManifest";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetManifestMetadata(containerName, manifestName);

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetStorageManifestFormsCorrectUrlAndMethod()
        {
            var containerName = "newContainer";
            var manifestName = "a/b/b/manifest";

            var client =
                new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetManifestMetadata(containerName, manifestName);

            Assert.AreEqual(string.Format("{0}/{1}/{2}?multipart-manifest=get", endpoint, containerName, manifestName), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task ErrorIsReturnedWhenManifestIsNotFound()
        {
            var containerName = "newContainer";
            var manifestName = "a/b/b/manifest";

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetManifestMetadata(containerName, manifestName);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetStorageManifest()
        {
            var containerName = "newContainer";
            var manifestName = "a/b/b/manifest";

            var content = TestHelper.CreateStream("[]");
            content.Position = 0;

            this.simulator.Objects.Add(manifestName, new StorageRestSimulator.StorageItem(manifestName) { Content = content });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetManifestMetadata(containerName, manifestName);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [TestMethod]
        public async Task CanGetStorageManifestWithMetadata()
        {
            var containerName = "newContainer";
            var manifestName = "a/b/b/manifest";

            var content = TestHelper.CreateStream(string.Empty);
            content.Position = 0;

            var metaData = new Dictionary<string, string> { { "X-Object-Meta-Test1", "Test1" }, { "X-Object-Meta-Test2", "Test2" } };

            this.simulator.Objects.Add(manifestName, new StorageRestSimulator.StorageItem(manifestName) { MetaData = metaData, Content = content });

            var client =
                 new StorageServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetManifestMetadata(containerName, manifestName);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            Assert.IsTrue(resp.Headers.Any(kvp => kvp.Key == "X-Object-Meta-Test2"));
            Assert.AreEqual("Test2", resp.Headers.First(kvp => kvp.Key == "X-Object-Meta-Test2").Value.First());
        }

        #endregion
    }
}
