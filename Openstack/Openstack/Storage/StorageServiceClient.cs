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

namespace Openstack.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Openstack.Common;
    using Openstack.Common.ServiceLocation;
    using Openstack.Identity;

    /// <inheritdoc/>
    internal class StorageServiceClient : IStorageServiceClient
    {
        internal StorageServiceClientContext Context;
        internal const string StorageServiceName = "Object Storage";

        public Uri GetPublicEndpoint()
        {
            //TODO: This should be removed as soon as the CLI can deprecate it's usage of it. 
            //      The reason is that this breaks encapsulation. The rest layer/client is responsible for resolving it's own endpoint,
            //      This object should not also try and resolve the uri. In general we abstracted the consumer away from the URI, we should not break that
            //      abstraction. 
            return this.Context.Credential.ServiceCatalog.GetPublicEndpoint(StorageServiceName, this.Context.Credential.Region);
        }

        /// <summary>
        /// Creates a new instance of the StorageServiceClient class.
        /// </summary>
        /// <param name="credentials">The credential to be used by this client.</param>
        /// <param name="token">The cancellation token to be used by this client.</param>
        public StorageServiceClient(IOpenstackCredential credentials, CancellationToken token)
        {
            this.Context = new StorageServiceClientContext(credentials, token, StorageServiceName);
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
            var validator = ServiceLocator.Instance.Locate<IStorageFolderNameValidator>();
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
                objects.Add(await client.GetStorageObject(containerName, storageObject.FullName));
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

        /// <summary>
        /// Gets a client to interact with the remote Openstack instance.
        /// </summary>
        /// <returns>A POCO client.</returns>
        internal IStorageServicePocoClient GetPocoClient()
        {
            return ServiceLocator.Instance.Locate<IStorageServicePocoClientFactory>().Create(this.Context);
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
