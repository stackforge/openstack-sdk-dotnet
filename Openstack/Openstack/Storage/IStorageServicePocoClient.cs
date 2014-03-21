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

    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Client that can interact with an Openstack storage service.
    /// </summary>
    public interface IStorageServicePocoClient
    {
        /// <summary>
        /// Creates a storage object on the remote Openstack instance.
        /// </summary>
        /// <param name="obj">The storage object to create.</param>
        /// <param name="content">The content of the storage object.</param>
        /// <returns>A storage object.</returns>
        Task<StorageObject> CreateStorageObject(StorageObject obj, Stream content);

        /// <summary>
        /// Creates a storage container on the remote Openstack instance.
        /// </summary>
        /// <param name="container">The storage container to create.</param>
        /// <returns>An async task.</returns>
        Task CreateStorageContainer(StorageContainer container);

        /// <summary>
        /// Gets the details of the current storage account.
        /// </summary>
        /// <returns>A storage account.</returns>
        Task<StorageAccount> GetStorageAccount();

        /// <summary>
        /// Gets the details of a storage container.
        /// </summary>
        /// <param name="containerName">The name of the storage container.</param>
        /// <returns>The storage container.</returns>
        Task<StorageContainer> GetStorageContainer(string containerName);

        /// <summary>
        /// Gets a storage object from the remote Openstack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent storage container.</param>
        /// <param name="objectName">The name of the storage object.</param>
        /// <returns>The storage object.</returns>
        Task<StorageObject> GetStorageObject(string containerName, string objectName);

        /// <summary>
        /// Downloads the content of a storage object from the remote Openstack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent storage container.</param>
        /// <param name="objectName">The name of the storage object.</param>
        /// <param name="outputStream">The output stream to copy the objects content to.</param>
        /// <returns>The details of the storage object.</returns>
        Task<StorageObject> DownloadStorageObject(string containerName, string objectName, Stream outputStream);

        /// <summary>
        /// Deletes a storage object from the remote Openstack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent storage container.</param>
        /// <param name="objectName">The name of the storage object.</param>
        /// <returns>An async task.</returns>
        Task DeleteStorageObject(string containerName, string objectName);

        /// <summary>
        /// Deletes a storage container from the remote Openstack instance.
        /// </summary>
        /// <param name="containerName">The name of the storage container.</param>
        /// <returns>An async task.</returns>
        Task DeleteStorageContainer(string containerName);

        /// <summary>
        /// Updates a storage container on the remote Openstack instance.
        /// </summary>
        /// <param name="container">The storage container to update.</param>
        /// <returns>An async task.</returns>
        Task UpdateStorageContainer(StorageContainer container);

        /// <summary>
        /// Updates a storage object on the remote Openstack instance.
        /// </summary>
        /// <param name="obj">The storage object to update.</param>
        /// <returns>An async task.</returns>
        Task UpdateStorageObject(StorageObject obj);

    }
}
