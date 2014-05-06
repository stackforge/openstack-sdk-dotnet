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
using System.Threading.Tasks;

namespace OpenStack.Storage
{
    /// <summary>
    /// Client that can interact with an OpenStack storage service.
    /// </summary>
    public interface IStorageServiceClient : IOpenStackServiceClient
    {
        /// <summary>
        /// Gets the threshold, in bytes, of what is considered a large object. 
        /// The threshold is used when determining when to split up a object into segments, and use a storage manifest to represent the segments of the object.
        /// </summary>
        long LargeObjectThreshold { get; set; }

        /// <summary>
        /// Gets the number of segments that will be created when a large object needs to be segmented and uploaded with a manifest. 
        /// </summary>
        int LargeObjectSegments { get; set; }

        /// <summary>
        /// The name of the container that will be used to hold the segments of large objects.
        /// </summary>
        string LargeObjectSegmentContainer { get; set; }

        /// <summary>
        /// Creates a storage container on the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="metadata">Metadata for the container.</param>
        /// <returns>An async task.</returns>
        Task CreateStorageContainer(string containerName, IDictionary<string, string> metadata);

        /// <summary>
        /// Gets a storage container from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>A storage container.</returns>
        Task<StorageContainer> GetStorageContainer(string containerName);

        /// <summary>
        /// Deletes a storage container from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>An async task.</returns>
        Task DeleteStorageContainer(string containerName);

        /// <summary>
        /// Updates a storage container on the remote OpenStack instance.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>An async task.</returns>
        Task UpdateStorageContainer(StorageContainer container);

        /// <summary>
        /// Lists the storage object in a given container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>An enumerable list of storage objects.</returns>
        Task<IEnumerable<StorageObject>> ListStorageObjects(string containerName);

        /// <summary>
        /// Lists storage containers on the remote OpenStack instance.
        /// </summary>
        /// <returns>An enumerable list of storage containers.</returns>
        Task<IEnumerable<StorageContainer>> ListStorageContainers();

        /// <summary>
        /// Gets the details of the current storage account from the remote OpenStack instance.
        /// </summary>
        /// <returns>A storage account.</returns>
        Task<StorageAccount> GetStorageAccount();

        /// <summary>
        /// Creates a storage object on the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <param name="metadata">Metadata for the object.</param>
        /// <param name="content">The objects content.</param>
        /// <returns>A storage object. </returns>
        Task<StorageObject> CreateStorageObject(string containerName, string objectName, IDictionary<string, string> metadata, Stream content);

        /// <summary>
        /// Creates a storage object on the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <param name="metadata">Metadata for the object.</param>
        /// <param name="content">The objects content.</param>
        /// <param name="numberOfsegments">The number of segments to use.</param>
        /// <returns>A storage object. </returns>
        Task<StorageObject> CreateLargeStorageObject(string containerName, string objectName, IDictionary<string, string> metadata, Stream content, int numberOfsegments);

        /// <summary>
        /// Gets a storage object from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <returns>The storage object.</returns>
        Task<StorageObject> GetStorageObject(string containerName, string objectName);

        /// <summary>
        /// Downloads the content of a storage object from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <param name="outputStream">The output stream to copy the objects content to.</param>
        /// <returns>The storage object details.</returns>
        Task<StorageObject> DownloadStorageObject(string containerName, string objectName, Stream outputStream);

        /// <summary>
        /// Deletes a storage object from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <returns>An async task.</returns>
        Task DeleteStorageObject(string containerName, string objectName);

        /// <summary>
        /// Updates a storage object on the remote OpenStack instance.
        /// </summary>
        /// <param name="obj">The object to update.</param>
        /// <returns>An async task.</returns>
        Task UpdateStorageObject(StorageObject obj);

        /// <summary>
        /// Creates a storage manifest on the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestName">The name of the manifest.</param>
        /// <param name="metadata">Metadata for the manifest.</param>
        /// <param name="objects">The list of storage objects.</param>
        /// <returns>An async task.</returns>
        Task<StorageManifest> CreateStorageManifest(string containerName, string manifestName, IDictionary<string, string> metadata, IEnumerable<StorageObject> objects);

        /// <summary>
        /// Creates a storage manifest on the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestName">The name of the manifest.</param>
        /// <param name="metadata">Metadata for the manifest.</param>
        /// <param name="segmentsPath">The path to the segment objects in the manifest.</param>
        /// <returns>An async task.</returns>
        Task<StorageManifest> CreateStorageManifest(string containerName, string manifestName, IDictionary<string, string> metadata, string segmentsPath);

        /// <summary>
        /// Gets a storage manifest from the remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the manifest.</param>
        /// <returns>The storage manifest.</returns>
        Task<StorageManifest> GetStorageManifest(string containerName, string objectName);

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
