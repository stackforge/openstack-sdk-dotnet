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
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Storage
{
    /// <inheritdoc/>
    internal class StorageServiceClient : IStorageServiceClient
    {
        internal ServiceClientContext Context;
        internal IServiceLocator ServiceLocator;

        /// <inheritdoc/>
        public long LargeObjectThreshold { get; set; }

        /// <inheritdoc/>
        public int LargeObjectSegments { get; set; }

        /// <inheritdoc/>
        public string LargeObjectSegmentContainer { get; set; }

        /// <summary>
        /// Creates a new instance of the StorageServiceClient class.
        /// </summary>
        /// <param name="credentials">The credential to be used by this client.</param>
        /// <param name="token">The cancellation token to be used by this client.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        public StorageServiceClient(IOpenStackCredential credentials, string serviceName, CancellationToken token, IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a storage service client with a null service locator.");

            this.LargeObjectThreshold = 524288000; //Set the default large file threshold to 500MB.
            this.LargeObjectSegments = 10; //set the default number of segments for large objects to 10.
            this.LargeObjectSegmentContainer = "LargeObjectSegments"; //set the default name of the container that will hold segments to 'LargeObjectSegments';

            this.ServiceLocator = serviceLocator;
            var endpoint = new Uri(credentials.ServiceCatalog.GetPublicEndpoint(serviceName, credentials.Region));
            this.Context = new ServiceClientContext(credentials, token, serviceName, endpoint);
        }

        /// <inheritdoc/>
        public async Task<StorageAccount> GetStorageAccount()
        {
            var client = this.GetPocoClient();
            return await client.GetStorageAccount();
        }

        /// <inheritdoc/>
        public async Task<StorageObject> CreateStorageObject(string containerName, string objectName, IDictionary<string, string> metadata, Stream content)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a storage object with a container name that is null or empty.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot create a storage object with a name that is null or empty.");
            content.AssertIsNotNull("content", "Cannot create a storage object with null content");

            if (content.Length > this.LargeObjectThreshold)
            {
                return await this.CreateLargeStorageObject(containerName, objectName, metadata, content, this.LargeObjectSegments);
            }

            //TODO: handle the content type better... 
            var requestObject = new StorageObject(objectName, containerName, "application/octet-stream", metadata);
            var client = this.GetPocoClient();
            return await client.CreateStorageObject(requestObject, content);
        }

        /// <inheritdoc/>
        public async Task CreateStorageContainer(string containerName, IDictionary<string, string> metadata)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a storage container with a container name that is null or empty.");
 
            var requestObject = new StorageContainer(containerName, metadata);
            var client = this.GetPocoClient();
            await client.CreateStorageContainer(requestObject);
        }

        /// <inheritdoc/>
        public async Task<StorageObject> GetStorageObject(string containerName, string objectName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot get a storage object with a container name that is null or empty.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot get a storage object with a name that is null or empty.");
 
            var client = this.GetPocoClient();
            return await client.GetStorageObject(containerName, objectName);
        }

        /// <inheritdoc/>
        public async Task<StorageObject> DownloadStorageObject(string containerName, string objectName, Stream outputStream)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "download create a storage object with a container name that is null or empty.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot download a storage object with a name that is null or empty.");
            outputStream.AssertIsNotNull("outputStream", "Cannot download a storage object with a null output stream.");

            var client = this.GetPocoClient();
            return await client.DownloadStorageObject(containerName, objectName, outputStream);
        }

        /// <inheritdoc/>
        public async Task DeleteStorageObject(string containerName, string objectName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot delete a storage object with a container name that is null or empty.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot delete a storage object with a name that is null or empty.");

            var client = this.GetPocoClient();
            await client.DeleteStorageObject(containerName, objectName);
        }

        /// <inheritdoc/>
        public async Task UpdateStorageObject(StorageObject obj)
        {
            obj.AssertIsNotNull("container", "Cannot update a storage object with a null object.");

            var client = this.GetPocoClient();
            await client.UpdateStorageObject(obj);
        }

        /// <inheritdoc/>
        public async Task<StorageManifest> CreateStorageManifest(string containerName, string manifestName, IDictionary<string, string> metadata, IEnumerable<StorageObject> objects)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a storage manifest with a container name that is null or empty.");
            manifestName.AssertIsNotNullOrEmpty("manifestNamee", "Cannot create a storage manifest with a name that is null or empty.");
            metadata.AssertIsNotNull("metadata","Cannot create a storage manifest with null metadata.");

            var client = this.GetPocoClient();

            var manifest = new StaticLargeObjectManifest(containerName, manifestName, metadata, objects.ToList());

            return await client.CreateStorageManifest(manifest);
        }

        /// <inheritdoc/>
        public async Task<StorageManifest> CreateStorageManifest(string containerName, string manifestName, IDictionary<string, string> metadata, string segmentsPath)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a storage manifest with a container name that is null or empty.");
            manifestName.AssertIsNotNullOrEmpty("manifestNamee", "Cannot create a storage manifest with a name that is null or empty.");
            metadata.AssertIsNotNull("metadata", "Cannot create a storage manifest with null metadata.");
            segmentsPath.AssertIsNotNullOrEmpty("segmentsPath", "Cannot create storage manifest with a null or empty segments path.");

            var client = this.GetPocoClient();

            var manifest = new  DynamicLargeObjectManifest(containerName, manifestName, metadata, segmentsPath);

            return await client.CreateStorageManifest(manifest);
        }

        /// <inheritdoc/>
        public async Task<StorageManifest> GetStorageManifest(string containerName, string manifestName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot get a storage manifest with a container name that is null or empty.");
            manifestName.AssertIsNotNullOrEmpty("manifestName", "Cannot get a storage manifest with a name that is null or empty.");

            var client = this.GetPocoClient();
            return await client.GetStorageManifest(containerName, manifestName);
        }

        /// <inheritdoc/>
        public async Task<StorageFolder> GetStorageFolder(string containerName, string folderName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot get a storage folder with a container name that is null or empty.");
            folderName.AssertIsNotNullOrEmpty("folderName", "Cannot get a storage folder with a name that is null or empty.");

            folderName = EnsureTrailingSlashOnFolderName(folderName);

            var client = this.GetPocoClient();
            return await client.GetStorageFolder(containerName, folderName);
        }

        /// <inheritdoc/>
        public async Task CreateStorageFolder(string containerName, string folderName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a storage folder with a container name that is null or empty.");
            folderName.AssertIsNotNullOrEmpty("folderName", "Cannot create a storage folder with a name that is null or empty.");

            folderName = EnsureTrailingSlashOnFolderName(folderName);
            var validator = this.ServiceLocator.Locate<IStorageFolderNameValidator>();
            if (!validator.Validate(folderName))
            {
                throw new ArgumentException(string.Format("Folder name '{0}' is invalid. Folder names cannot includes  consecutive slashes.", folderName), "folderName");
            }

            var client = this.GetPocoClient();
            await client.CreateStorageFolder(containerName, folderName);
        }

        /// <inheritdoc/>
        public async Task DeleteStorageFolder(string containerName, string folderName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot delete a storage folder with a container name that is null or empty.");
            folderName.AssertIsNotNullOrEmpty("folderName", "Cannot delete a storage folder with a name that is null or empty.");

            folderName = EnsureTrailingSlashOnFolderName(folderName);

            var client = this.GetPocoClient();
            await client.DeleteStorageFolder(containerName, folderName);
        }

        /// <inheritdoc/>
        public async Task<StorageContainer> GetStorageContainer(string containerName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot get a storage container with a container name that is null or empty.");

            var client = this.GetPocoClient();
            return await client.GetStorageContainer(containerName);
        }

        /// <inheritdoc/>
        public async Task DeleteStorageContainer(string containerName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot delete a storage container with a container name that is null or empty.");

            var client = this.GetPocoClient();
            await client.DeleteStorageContainer(containerName);
        }

        /// <inheritdoc/>
        public async Task UpdateStorageContainer(StorageContainer container)
        {
            container.AssertIsNotNull("container", "Cannot update a storage container with a null container.");

            var client = this.GetPocoClient();
            await client.UpdateStorageContainer(container);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<StorageObject>> ListStorageObjects(string containerName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot list storage objects with a container name that is null or empty.");

            var objects = new List<StorageObject>();
            var client = this.GetPocoClient();

            var container = await client.GetStorageContainer(containerName);
            foreach (var storageObject in container.Objects)
            {
                try
                {
                    var obj = await client.GetStorageObject(containerName, storageObject.FullName);
                    objects.Add(obj);
                }
                catch (InvalidOperationException ex)
                {
                    //TODO: think about a better way to bubble up non-fatal errors like a 404.
                    if (!ex.Message.Contains(HttpStatusCode.NotFound.ToString()))
                    {
                        throw;
                    }
                }
                
            }

            return objects;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<StorageContainer>> ListStorageContainers()
        {
            var contianers = new List<StorageContainer>();
            var client = this.GetPocoClient();

            var account = await client.GetStorageAccount();
            foreach (var storageContainer in account.Containers)
            {
                contianers.Add(await client.GetStorageContainer(storageContainer.Name));
            }

            return contianers;
        }

        /// <inheritdoc/>
        public async Task<StorageObject> CreateLargeStorageObject(string containerName, string objectName, IDictionary<string, string> metadata, Stream content, int numberOfSegments)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a large storage object with a null or empty container name.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot create a large storage object with a null or empty name.");
            metadata.AssertIsNotNull("metadata", "Cannot create a large storage object with null metadata.");
            content.AssertIsNotNull("content", "Cannot create a large storage object with null content.");

            if (numberOfSegments <= 0)
            {
                throw new ArgumentException("Cannot create a large object with zero or less segments.", "numberOfSegments");
            }

            var factory = this.ServiceLocator.Locate<ILargeStorageObjectCreatorFactory>();
            var creator = factory.Create(this);
            return await creator.Create(containerName, objectName, metadata, content, numberOfSegments, this.LargeObjectSegmentContainer);
        }

        /// <summary>
        /// Gets a client to interact with the remote OpenStack instance.
        /// </summary>
        /// <returns>A POCO client.</returns>
        internal IStorageServicePocoClient GetPocoClient()
        {
            return this.ServiceLocator.Locate<IStorageServicePocoClientFactory>().Create(this.Context, this.ServiceLocator);
        }

        /// <summary>
        /// Ensures that a folder name has a single trailing slash on the end.
        /// </summary>
        /// <param name="folderName">The folder name.</param>
        /// <returns>The folder name with a slash on the end.</returns>
        internal string EnsureTrailingSlashOnFolderName(string folderName)
        {
            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }
            return folderName;
        }
    }
}
