// /* ============================================================================
// Copyright 2014 Hewlett Packard
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Identity;
using OpenStack.Storage;

namespace OpenStack.Test.Functional
{
    /// <summary>
    /// Storage based scenario tests
    /// </summary>
    [TestClass]
    public class StorageTests
    {
        private static IOpenStackCredential _credential;
        private static IOpenStackClient _client;

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The test context.</param>
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _credential = new OpenStackCredential(Configuration.AuthUri, Configuration.UserName, Configuration.Password, Configuration.TenantId);
            _client = OpenStackClientFactory.CreateClient(_credential);

            _client.Connect().Wait();

            Assert.IsNotNull(_credential.AccessTokenId, "No AccessTokenId was set, authentication failed.");
            Assert.AreNotEqual(0, _credential.ServiceCatalog.Count, "ServiceCatalog should not be empty.");
        }

        /// <summary>
        /// This method contains a suite of scenario tests. They are lumped together because they are dependent on each other,
        /// and because the setup/cleanup time would be expensive if they were separated out.
        /// </summary>
        [TestMethod]
        public void ScenarioTests()
        {
            var storageClient =
                _client.CreateServiceClientByName<IStorageServiceClient>(Configuration.StorageServiceName);
            var getAccountTask = storageClient.GetStorageAccount();

            try
            {
                // Verify that we can get an empty storage account to use.
                getAccountTask.Wait();
                StorageAccount account = getAccountTask.Result;

                Assert.IsNotNull(account.Name, "Unable to get StorageClient and StorageAccount.");
                Assert.AreEqual(0, ((List<StorageContainer>) account.Containers).Count,
                    "Expected storage account to contain no containers.");

                // Verify that we can create a container and then get it.
                storageClient.CreateStorageContainer(Configuration.ContainerName, new Dictionary<string, string>())
                    .Wait();

                var listContainersTask = storageClient.ListStorageContainers();
                listContainersTask.Wait();
                List<StorageContainer> containers = (List<StorageContainer>) listContainersTask.Result;

                Assert.AreEqual(1, containers.Count, "Expected to find only the container we created.");
                Assert.AreEqual(Configuration.ContainerName, containers[0].Name, "Unexpected container name.");

                // Verify that we can create a folder, and that it is returned in ListStorageObjects.
                storageClient.CreateStorageFolder(Configuration.ContainerName, Configuration.FolderName).Wait();

                var listObjectsTask = storageClient.ListStorageObjects(Configuration.ContainerName);
                listObjectsTask.Wait();
                List<StorageObject> objects = (List<StorageObject>) listObjectsTask.Result;

                Assert.AreEqual(1, objects.Count, "Expected to find the folder we just created.");
                Assert.AreEqual(Configuration.FolderName, objects[0].Name, "Expected a folder with name TestFolder.");

                // Verify that we can create and download an object, and that they match.
                storageClient.CreateStorageObject(
                    Configuration.ContainerName,
                    Configuration.ObjectName,
                    new Dictionary<string, string>(),
                    new MemoryStream(Encoding.ASCII.GetBytes("Test Content"))
                    ).Wait();

                var ms = new MemoryStream();
                var mr = new StreamReader(ms);

                storageClient.DownloadStorageObject(Configuration.ContainerName, Configuration.ObjectName, ms).Wait();
                ms.Position = 0;
                Assert.AreEqual("Test Content", mr.ReadToEnd(), "The stream we uploaded wasn't what was returned.");
            }
            finally
            {
                storageClient.DeleteStorageObject(Configuration.ContainerName, Configuration.ObjectName).Wait();
                storageClient.DeleteStorageFolder(Configuration.ContainerName, Configuration.FolderName).Wait();
                storageClient.DeleteStorageContainer(Configuration.ContainerName).Wait();
            }
        }
    }
}