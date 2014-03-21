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
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Openstack.Common;
    using Openstack.Common.ServiceLocation;

    /// <inheritdoc/>
    internal class StorageServicePocoClient : IStorageServicePocoClient
    {
        internal StorageServiceClientContext _context;

        /// <summary>
        /// Creates a new instance of the StorageServicePocoClient class.
        /// </summary>
        /// <param name="context">The storage service to use for this client.</param>
        internal StorageServicePocoClient(StorageServiceClientContext context)
        {
            this._context = context;
        }

        /// <inheritdoc/>
        public async Task<StorageObject> CreateStorageObject(StorageObject obj, Stream content)
        {
            obj.ContainerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a storage object with a null or empty container name.");
            obj.AssertIsNotNull("obj","Cannot Create a null storage object.");
            obj.Name.AssertIsNotNullOrEmpty("obj.Name","Cannot create a storage object without a name.");

            var client = this.GetRestClient();
            var resp = await client.CreateObject(obj.ContainerName, obj.Name, obj.Metadata, content);

            if (resp.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Failed to create storage object '{0}'. The remote server returned the following status code: '{1}'.", obj.Name, resp.StatusCode));
            }

            var converter = ServiceLocator.Instance.Locate<IStorageObjectPayloadConverter>();
            var respObj = converter.Convert(obj.ContainerName, obj.Name, resp.Headers);

            return respObj;
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

            var endpoint = this._context.Credential.ServiceCatalog.GetPublicEndpoint(this._context.StorageServiceName, this._context.Region);
            var accountName = endpoint.Segments.Last().TrimEnd('/');

            var converter = ServiceLocator.Instance.Locate<IStorageAccountPayloadConverter>();
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

            var converter = ServiceLocator.Instance.Locate<IStorageContainerPayloadConverter>();
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

            var converter = ServiceLocator.Instance.Locate<IStorageObjectPayloadConverter>();
            var obj = converter.Convert(containerName, objectName, resp.Headers);

            return obj;
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

            var converter = ServiceLocator.Instance.Locate<IStorageObjectPayloadConverter>();
            var obj = converter.Convert(containerName, objectName, resp.Headers);

            await resp.Content.CopyToAsync(outputStream);
            outputStream.Position = 0;

            return obj;
        }

        /// <inheritdoc/>
        public async Task DeleteStorageObject(string containerName, string objectName)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot delete a storage object with a container name that is null or empty.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot delete a storage object with a name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.DeleteObject(containerName, objectName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to delete storage object '{0}'. The remote server returned the following status code: '{1}'.", objectName, resp.StatusCode));
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
        public async Task UpdateStorageObject(StorageObject obj)
        {
            obj.ContainerName.AssertIsNotNullOrEmpty("containerName", "Cannot update a storage object with a container name that is null or empty.");
            obj.Name.AssertIsNotNullOrEmpty("objectName", "Cannot update a storage object with a name that is null or empty.");

            var client = this.GetRestClient();
            var resp = await client.UpdateObject(obj.ContainerName, obj.Name, obj.Metadata);

            if (resp.StatusCode != HttpStatusCode.Accepted)
            {
                throw new InvalidOperationException(string.Format("Failed to update storage object '{0}'. The remote server returned the following status code: '{1}'.", obj.Name, resp.StatusCode));
            }
        }

        /// <summary>
        /// Gets a client that can be used to connect to the REST endpoints of an Openstack storage service.
        /// </summary>
        /// <returns>The client.</returns>
        internal IStorageServiceRestClient GetRestClient()
        {
            return ServiceLocator.Instance.Locate<IStorageServiceRestClientFactory>().Create(this._context);
        }
    }
}
