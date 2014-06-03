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
using System.Threading.Tasks;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Storage
{
    /// <inheritdoc/>
    internal class StorageServicePocoClient : IStorageServicePocoClient
    {
        internal ServiceClientContext _context;
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the StorageServicePocoClient class.
        /// </summary>
        /// <param name="context">The storage service to use for this client.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        internal StorageServicePocoClient(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a storage service poco client with a null service locator.");

            this._context = context;
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public async Task<StorageObject> CreateStorageObject(StorageObject obj, Stream content)
        {
            obj.AssertIsNotNull("obj", "Cannot create a null storage object.");
            obj.ContainerName.AssertIsNotNullOrEmpty("obj.ContainerName", "Cannot create a storage object with a null or empty container name.");
            obj.Name.AssertIsNotNullOrEmpty("obj.Name","Cannot create a storage object without a name.");

            var client = this.GetRestClient();
            var resp = await client.CreateObject(obj.ContainerName, obj.FullName, obj.Metadata, content);

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to create storage object '{0}'. The remote server returned the following status code: '{1}'.", obj.Name, resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IStorageObjectPayloadConverter>();
            var respObj = converter.Convert(obj.ContainerName, obj.FullName, resp.Headers);

            return respObj;
        }

        public async Task<StorageManifest> CreateStorageManifest(StorageManifest manifest)
        {
            manifest.AssertIsNotNull("manifest", "Cannot create a null storage manifest.");
            manifest.ContainerName.AssertIsNotNullOrEmpty("manifest.ContainerName", "Cannot create a storage manifest with a null or empty container name.");
            manifest.Name.AssertIsNotNullOrEmpty("manifest.Name", "Cannot create a storage manifest without a name.");

            var client = this.GetRestClient();
            IHttpResponseAbstraction resp;

            var dynamicManifest = manifest as DynamicLargeObjectManifest;
            var staticManifest = manifest as StaticLargeObjectManifest;

            if (dynamicManifest == null && staticManifest == null)
            {
                throw new InvalidOperationException(string.Format("Failed to create storage manifest '{0}'. The given manifest type is not supported: '{1}'.", manifest.Name, manifest.GetType().Name));
            }

            if (dynamicManifest != null) //dynamic large object manifest
            {
                resp = await client.CreateDynamicManifest(dynamicManifest.ContainerName, dynamicManifest.FullName, dynamicManifest.Metadata, dynamicManifest.SegmentsPath);
            }
            else //static large object manifest
            {
                var converter = this.ServiceLocator.Locate<IStorageObjectPayloadConverter>();
                var manifestPayload = converter.Convert(staticManifest.Objects).ConvertToStream();

                resp = await client.CreateStaticManifest(staticManifest.ContainerName, staticManifest.FullName, staticManifest.Metadata, manifestPayload);
            }

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to create storage manifest '{0}'. The remote server returned the following status code: '{1}'.", manifest.Name, resp.StatusCode));
            }

            return await this.GetStorageManifest(manifest.ContainerName, manifest.FullName);
        }

        /// <inheritdoc/>
        public async Task CreateStorageContainer(StorageContainer container)
        {
            container.AssertIsNotNull("container", "Cannot Create a null storage container.");
            container.Name.AssertIsNotNullOrEmpty("container.Name", "Cannot create a storage container without a name.");

            var client = this.GetRestClient();
            var resp = await client.CreateContainer(container.Name, container.Metadata);

            if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to create storage container '{0}'. The remote server returned the following status code: '{1}'.", container.Name, resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task<StorageAccount> GetStorageAccount()
        {
            var client = this.GetRestClient();
            var resp = await client.GetAccount();

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to get storage account. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var accountName = _context.PublicEndpoint.Segments.Last().TrimEnd('/');

            var converter = this.ServiceLocator.Locate<IStorageAccountPayloadConverter>();
            var account = converter.Convert(accountName, resp.Headers, await resp.ReadContentAsStringAsync());

            return account;
        }

        /// <inheritdoc/>
        public async Task<StorageContainer> GetStorageContainer(string containerName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot get a storage container with a name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.GetContainer(containerName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to get storage container '{0}'. The remote server returned the following status code: '{1}'.", containerName, resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IStorageContainerPayloadConverter>();
            var container = converter.Convert(containerName, resp.Headers, await resp.ReadContentAsStringAsync());

            return container;
        }

        /// <inheritdoc/>
        public async Task<StorageObject> GetStorageObject(string containerName, string objectName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot get a storage object with a container name that is null or empty.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot get a storage object with a name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.GetObjectMetadata(containerName, objectName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to get storage object '{0}'. The remote server returned the following status code: '{1}'.", objectName, resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IStorageObjectPayloadConverter>();
            var obj = converter.Convert(containerName, objectName, resp.Headers);

            return obj;
        }

        public async Task<StorageManifest> GetStorageManifest(string containerName, string manifestName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot get a storage manifest with a container name that is null or empty.");
            manifestName.AssertIsNotNullOrEmpty("manifestName", "Cannot get a storage manifest with a name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.GetManifestMetadata(containerName, manifestName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to get storage manifest '{0}'. The remote server returned the following status code: '{1}'.", manifestName, resp.StatusCode));
            }

            var objectConverter = this.ServiceLocator.Locate<IStorageObjectPayloadConverter>();
            var obj = objectConverter.Convert(containerName, manifestName, resp.Headers);

            if (!(obj is StorageManifest))
            {
                throw new InvalidOperationException(string.Format("Failed to get storage manifest '{0}'. The requested object is not a manifest.", manifestName));
            }

            if (obj is StaticLargeObjectManifest)
            {
                var manifest = obj as StaticLargeObjectManifest;
                manifest.Objects = objectConverter.Convert(containerName, await resp.ReadContentAsStringAsync()).ToList();
            }

            return obj as StorageManifest;
        }

        /// <inheritdoc/>
        public async Task<StorageObject> DownloadStorageObject(string containerName, string objectName, Stream outputStream)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot download a storage object with a container name that is null or empty.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot download a storage object with a name that is null or empty.");
            outputStream.AssertIsNotNull("outputStream","Cannot download a storage object with a null output stream.");

            var client = this.GetRestClient();
            var resp = await client.GetObject(containerName, objectName);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to download storage object '{0}'. The remote server returned the following status code: '{1}'.", objectName, resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IStorageObjectPayloadConverter>();
            var obj = converter.Convert(containerName, objectName, resp.Headers);

            await resp.Content.CopyAsync(outputStream);
            outputStream.Position = 0;

            return obj;
        }

        /// <inheritdoc/>
        public async Task DeleteStorageObject(string containerName, string itemName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot delete a storage object with a container name that is null or empty.");
            itemName.AssertIsNotNullOrEmpty("objectName", "Cannot delete a storage object with a name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.DeleteObject(containerName, itemName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to delete storage object '{0}'. The remote server returned the following status code: '{1}'.", itemName, resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task DeleteStorageContainer(string containerName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot delete a storage container with a container name that is null or empty.");
            
            var client = this.GetRestClient();
            var resp = await client.DeleteContainer(containerName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to delete storage container '{0}'. The remote server returned the following status code: '{1}'.", containerName, resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task UpdateStorageContainer(StorageContainer container)
        {
            container.Name.AssertIsNotNullOrEmpty("container.Name", "Cannot update a storage container with a name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.UpdateContainer(container.Name, container.Metadata);

            if (resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to update storage container '{0}'. The remote server returned the following status code: '{1}'.", container.Name, resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task UpdateStorageObject(StorageObject item)
        {
            item.ContainerName.AssertIsNotNullOrEmpty("containerName", "Cannot update a storage object with a container name that is null or empty.");
            item.Name.AssertIsNotNullOrEmpty("objectName", "Cannot update a storage object with a name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.UpdateObject(item.ContainerName, item.Name, item.Metadata);

            if (resp.StatusCode != HttpStatusCode.Accepted)
            {
                throw new InvalidOperationException(string.Format("Failed to update storage object '{0}'. The remote server returned the following status code: '{1}'.", item.Name, resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task<StorageFolder> GetStorageFolder(string containerName, string folderName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot get a storage folder with a container name that is null or empty.");
            folderName.AssertIsNotNullOrEmpty("folderName", "Cannot get a storage folder with a folder name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.GetFolder(containerName, folderName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to get storage folder '{0}'. The remote server returned the following status code: '{1}'.", folderName, resp.StatusCode));
            }

            try
            {
                var converter = this.ServiceLocator.Locate<IStorageFolderPayloadConverter>();
                var folder = converter.Convert(containerName, folderName, await resp.ReadContentAsStringAsync());
                return folder;
            }
            catch (InvalidDataException)
            {
                throw new InvalidOperationException(string.Format("Failed to get storage folder '{0}'. The requested folder could not be found.", folderName));
            }
        }

        /// <inheritdoc/>
        public async Task CreateStorageFolder(string containerName, string folderName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a storage folder with a container name that is null or empty.");
            folderName.AssertIsNotNullOrEmpty("folderName", "Cannot create a storage folder with a folder name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.CreateObject(containerName, folderName, new Dictionary<string, string>(), new MemoryStream());

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to create storage folder '{0}'. The remote server returned the following status code: '{1}'.", folderName, resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task DeleteStorageFolder(string containerName, string folderName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot delete a storage object with a container name that is null or empty.");
            folderName.AssertIsNotNullOrEmpty("objectName", "Cannot delete a storage object with a name that is null or empty.");

            var folder = await this.GetStorageFolder(containerName, folderName);
            if (folder.Folders.Count > 0 || folder.Objects.Count > 0)
            {
                throw new InvalidOperationException(string.Format("Failed to delete storage folder '{0}'. The folder is not empty and cannot be deleted.", folderName));
            }

            var client = this.GetRestClient();
            var resp = await client.DeleteObject(containerName, folderName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to delete storage folder '{0}'. The remote server returned the following status code: '{1}'.", folderName, resp.StatusCode));
            }
        }

        /// <summary>
        /// Gets a client that can be used to connect to the REST endpoints of an OpenStack storage service.
        /// </summary>
        /// <returns>The client.</returns>
        internal IStorageServiceRestClient GetRestClient()
        {
            return this.ServiceLocator.Locate<IStorageServiceRestClientFactory>().Create(this._context, this.ServiceLocator);
        }
    }
}
