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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    public class TestStorageServiceClient : IStorageServiceClient
    {
        public long LargeObjectThreshold { get; set; }
        public int LargeObjectSegments { get; set; }
        public string LargeObjectSegmentContainer { get; set; }

        public Func<string, IDictionary<string, string>, Task> CreateStorageContainerDelegate { get; set; }

        public Func<string, Task<StorageContainer>> GetStorageContainerDelegate { get; set; }

        public Func<string, Task> DeleteContainerDelegate { get; set; }

        public Func<StorageContainer, Task> UpdateStorageContainerDelegate { get; set; }

        public Func<string, Task<IEnumerable<StorageObject>>> ListStorageObjectsDelegate { get; set; }

        public Func<Task<IEnumerable<StorageContainer>>> ListStorageContainersDelegate { get; set; }

        public Func<Task<StorageAccount>> GetStorageAccountDelegate { get; set; }

        public Func<string, string, IDictionary<string, string>, Stream, Task<StorageObject>> CreateStorageObjectDelegate { get; set; }

        public Func<string, string, IDictionary<string, string>, Stream, int, Task<StorageObject>> CreateLargeStorageObjectDelegate { get; set; }

        public Func<string, string, Task<StorageObject>> GetStorageObjectDelegate { get; set; }

        public Func<string, string, Stream, Task<StorageObject>> DownloadStorageObjectDelegate { get; set; }

        public Func<string, string, Task> DeleteStorageObjectDelegate { get; set; }

        public Func<StorageObject, Task> UpdateStorageObjectDelegate { get; set; }

        public Func<string, string, IDictionary<string, string>, IEnumerable<StorageObject>, Task<StorageManifest>> CreateStaticStorageManifestDelegate { get; set; }

        public Func<string, string, IDictionary<string, string>, string, Task<StorageManifest>> CreateDynamicStorageManifestDelegate { get; set; }

        public Func<string, string, Task<StorageManifest>> GetStorageManifestDelegate { get; set; }

        public Func<string, string, Task<StorageFolder>> GetStorageFolderDelegate { get; set; }

        public Func<string, string, Task> CreateStorageFolderDelegate { get; set; }


        public Func<string, string, Task> DeleteStorageFolderDelegate { get; set; }

        public Func<Uri> GetPublicEndpointDelegate { get; set; }

        public Uri GetPublicEndpoint()
        {
            return GetPublicEndpointDelegate();
        }

        public async Task CreateStorageContainer(string containerName, IDictionary<string, string> metadata)
        {
            await CreateStorageContainerDelegate(containerName, metadata);
        }

        public async Task<StorageContainer> GetStorageContainer(string containerName)
        {
            return await GetStorageContainerDelegate(containerName);
        }

        public async Task DeleteStorageContainer(string containerName)
        {
            await DeleteContainerDelegate(containerName);
        }

        public async Task UpdateStorageContainer(StorageContainer container)
        {
            await UpdateStorageContainerDelegate(container);
        }

        public async Task<IEnumerable<StorageObject>> ListStorageObjects(string containerName)
        {
            return await ListStorageObjectsDelegate(containerName);
        }

        public async Task<IEnumerable<StorageContainer>> ListStorageContainers()
        {
            return await ListStorageContainersDelegate();
        }

        public async Task<StorageAccount> GetStorageAccount()
        {
            return await GetStorageAccountDelegate();
        }

        public async Task<StorageObject> CreateStorageObject(string containerName, string objectName, IDictionary<string, string> metadata, Stream content)
        {
            return await CreateStorageObjectDelegate(containerName, objectName, metadata, content);
        }

        public async Task<StorageObject> CreateLargeStorageObject(string containerName, string objectName, IDictionary<string, string> metadata, Stream content,
            int numberOfsegments)
        {
            return
                await CreateLargeStorageObjectDelegate(containerName, objectName, metadata, content, numberOfsegments);
        }

        public async Task<StorageObject> GetStorageObject(string containerName, string objectName)
        {
            return await GetStorageObjectDelegate(containerName, objectName);
        }

        public async Task<StorageObject> DownloadStorageObject(string containerName, string objectName, Stream outputStream)
        {
            return await DownloadStorageObjectDelegate(containerName, objectName, outputStream);
        }

        public async Task DeleteStorageObject(string containerName, string objectName)
        {
            await DeleteStorageObjectDelegate(containerName, objectName);
        }

        public async Task UpdateStorageObject(StorageObject obj)
        {
            await UpdateStorageObject(obj);
        }

        public async Task<StorageManifest> CreateStorageManifest(string containerName, string manifestName, IDictionary<string, string> metadata, IEnumerable<StorageObject> objects)
        {
            return await CreateStaticStorageManifestDelegate(containerName, manifestName, metadata, objects);
        }

        public async Task<StorageManifest> CreateStorageManifest(string containerName, string manifestName, IDictionary<string, string> metadata, string segmentsPath)
        {
            return await CreateDynamicStorageManifestDelegate(containerName, manifestName, metadata, segmentsPath);
        }

        public async Task<StorageManifest> GetStorageManifest(string containerName, string objectName)
        {
            return await GetStorageManifestDelegate(containerName, objectName);
        }

        public async Task<StorageFolder> GetStorageFolder(string containerName, string folderName)
        {
            return await GetStorageFolderDelegate(containerName, folderName);
        }

        public async Task CreateStorageFolder(string containerName, string folderName)
        {
            await this.CreateStorageFolderDelegate(containerName, folderName);
        }

        public async Task DeleteStorageFolder(string containerName, string folderName)
        {
            await DeleteStorageFolderDelegate(containerName, folderName);
        }
    }
}
