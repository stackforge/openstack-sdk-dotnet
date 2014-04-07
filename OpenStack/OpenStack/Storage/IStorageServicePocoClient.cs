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

using System.IO;
using System.Threading.Tasks;

namespace OpenStack.Storage
{
    /// <summary>
    /// Client that can interact with an OpenStack storage service.
    /// </summary>
    public interface IStorageServicePocoClient
    {
        /// <summary>
        /// Creates a storage object on the remote OpenStack instance.
        /// </summary>
        /// <param name="obj">The storage object to create.</param>
        /// <param name="content">The content of the storage object.</param>
        /// <returns>A storage object.</returns>
        Task<StorageObject> CreateStorageObject(StorageObject obj, Stream content);

        /// <summary>
        /// Creates a storage manifest on the remote OpenStack instance.
        /// </summary>
        /// <param name="manifest">The storage manifest to create.</param>
        /// <returns>A storage manifest.</returns>
        Task<StorageManifest> CreateStorageManifest(StorageManifest manifest);

        /// <summary>
        /// Creates a storage container on the remote OpenStack instance.
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
        /// Gets a storage object from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent storage container.</param>
        /// <param name="objectName">The name of the storage object.</param>
        /// <returns>The storage object.</returns>
        Task<StorageObject> GetStorageObject(string containerName, string objectName);

        /// <summary>
        /// Gets a storage manifest from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent storage container.</param>
        /// <param name="manifestName">The name of the storage manifest.</param>
        /// <returns>The storage manifest.</returns>
        Task<StorageManifest> GetStorageManifest(string containerName, string manifestName);

        /// <summary>
        /// Downloads the content of a storage object from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent storage container.</param>
        /// <param name="objectName">The name of the storage object.</param>
        /// <param name="outputStream">The output stream to copy the objects content to.</param>
        /// <returns>The details of the storage object.</returns>
        Task<StorageObject> DownloadStorageObject(string containerName, string objectName, Stream outputStream);

        /// <summary>
        /// Deletes a storage object from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent storage container.</param>
        /// <param name="objectName">The name of the object to delete.</param>
        /// <returns>An async task.</returns>
        Task DeleteStorageObject(string containerName, string objectName);

        /// <summary>
        /// Deletes a storage container from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the storage container.</param>
        /// <returns>An async task.</returns>
        Task DeleteStorageContainer(string containerName);

        /// <summary>
        /// Updates a storage container on the remote OpenStack instance.
        /// </summary>
        /// <param name="container">The storage container to update.</param>
        /// <returns>An async task.</returns>
        Task UpdateStorageContainer(StorageContainer container);

        /// <summary>
        /// Updates a storage object on the remote OpenStack instance.
        /// </summary>
        /// <param name="obj">The storage object to update.</param>
        /// <returns>An async task.</returns>
        Task UpdateStorageObject(StorageObject obj);

        /// <summary>
        /// Gets a storage folder from the remote OpenStack instance. The returned folder is a shallow object graph representation.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="folderName">The name of the folder to get.</param>
        /// <returns>A shallow object representation of the folder and it's contained objects and sub folders.</returns>
        Task<StorageFolder> GetStorageFolder(string containerName, string folderName);

        /// <summary>
        /// Creates a storage folder on the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="folderName">The name of the folder to create.</param>
        /// <returns>An async task.</returns>
        Task CreateStorageFolder(string containerName, string folderName);

        /// <summary>
        /// Deletes a storage folder from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="folderName">The name of the folder to delete.</param>
        /// <returns>An async task.</returns>
        Task DeleteStorageFolder(string containerName, string folderName);
    }
}
