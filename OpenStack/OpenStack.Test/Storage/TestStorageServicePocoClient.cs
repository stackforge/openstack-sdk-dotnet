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
using System.IO;
using System.Threading.Tasks;
using OpenStack.Common.ServiceLocation;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    public class TestStorageServicePocoClient : IStorageServicePocoClient
    {
        public Func<StorageObject, Stream, Task<StorageObject>> CreateStorageObjectDelegate { get; set; }

        public Func<StorageManifest,Task<StorageManifest>> CreateStorageManifestDelegate { get; set; }

        public Func<StorageContainer, Task<StorageContainer>> CreateStorageContainerDelegate { get; set; }

        public Func<string, string, Task> CreateStorageFolderDelegate { get; set; }

        public Func<string, Task<StorageContainer>> GetStorageContainerDelegate { get; set; }

        public Func<Task<StorageAccount>> GetStorageAccountDelegate { get; set; }

        public Func<string, string, Task<StorageObject>> GetStorageObjectDelegate { get; set; }

        public Func<string, string, Task<StorageManifest>> GetStorageManifestDelegate { get; set; }

        public Func<string, string, Task<StorageFolder>> GetStorageFolderDelegate { get; set; }

        public Func<string, string, Stream, Task<StorageObject>> DownloadStorageObjectDelegate { get; set; }

        public Func<string, string, Task> DeleteStorageObjectDelegate { get; set; }

        public Func<string, string, Task> DeleteStorageFolderDelegate { get; set; }

        public Func<StorageObject, Task> UpdateStorageObjectDelegate { get; set; }

        public Func<string, Task> DeleteStorageConainerDelegate { get; set; }

        public Func<StorageContainer, Task> UpdateStorageContainerDelegate { get; set; }

        public async Task<StorageObject> CreateStorageObject(StorageObject obj, Stream content)
        {
            return await this.CreateStorageObjectDelegate(obj, content);
        }

        public async Task<StorageManifest> CreateStorageManifest(StorageManifest manifest)
        {
            return await this.CreateStorageManifestDelegate(manifest);
        }

        public async Task CreateStorageContainer(StorageContainer container)
        {
            await this.CreateStorageContainerDelegate(container);
        }

        public async Task<StorageAccount> GetStorageAccount()
        {
            return await this.GetStorageAccountDelegate();
        }

        public async Task<StorageContainer> GetStorageContainer(string containerName)
        {
            return await this.GetStorageContainerDelegate(containerName);
        }

        public async Task<StorageObject> GetStorageObject(string containerName, string objectName)
        {
            return await this.GetStorageObjectDelegate(containerName, objectName);
        }

        public async Task<StorageManifest> GetStorageManifest(string containerName, string manifestName)
        {
            return await this.GetStorageManifestDelegate(containerName, manifestName);
        }

        public async Task<StorageObject> DownloadStorageObject(string containerName, string objectName, Stream outputStream)
        {
            return await this.DownloadStorageObjectDelegate(containerName, objectName, outputStream);
        }

        public async Task DeleteStorageObject(string containerName, string itemName)
        {
            await this.DeleteStorageObjectDelegate(containerName, itemName);
        }

        public async Task DeleteStorageContainer(string containerName)
        {
            await this.DeleteStorageConainerDelegate(containerName);
        }

        public async Task UpdateStorageContainer(StorageContainer container)
        {
            await this.UpdateStorageContainerDelegate(container);
        }

        public async Task UpdateStorageObject(StorageObject item)
        {
            await this.UpdateStorageObjectDelegate(item);
        }

        public async Task<StorageFolder> GetStorageFolder(string containerName, string folderName)
        {
            return await this.GetStorageFolderDelegate(containerName, folderName);
        }

        public async Task CreateStorageFolder(string containerName, string folderName)
        {
            await this.CreateStorageFolderDelegate(containerName, folderName);
        }

        public async Task DeleteStorageFolder(string containerName, string folderName)
        {
            await this.DeleteStorageFolderDelegate(containerName, folderName);
        }
    }

    public class TestStorageServicePocoClientFactory : IStorageServicePocoClientFactory
    {
        internal IStorageServicePocoClient client;

        public TestStorageServicePocoClientFactory(IStorageServicePocoClient client)
        {
            this.client = client;
        }

        public IStorageServicePocoClient Create(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            return client;
        }
    }
}
