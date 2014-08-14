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
using System.Threading.Tasks;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    public class TestComputeServicePocoClient : IComputeServicePocoClient
    {
        public Func<string, Task<ComputeFlavor>> GetFlavorDelegate { get; set; }

        public Func<Task<IEnumerable<ComputeFlavor>>> GetFlavorsDelegate { get; set; }

        public Func<string, Task<ComputeImage>> GetImageDelegate { get; set; }

        public Func<Task<IEnumerable<ComputeImage>>> GetImagesDelegate { get; set; }

        public Func<string, Task> DeleteImageDelegate { get; set; }

        public Func<string, string, string, string, string, IEnumerable<string>,  Task<ComputeServer>> CreateServerDelegate { get; set; }

        public Func<string, Task> DeleteServerDelegate { get; set; }

        public Func<string, Task<IDictionary<string,string>>> GetImageMetadataDelegate { get; set; }

        public Func<string, IDictionary<string, string>, Task> UpdateImageMetadataDelegate { get; set; }

        public Func<string, string, Task> DeleteImageMetadataDelegate { get; set; }

        public Func<string, Task<IDictionary<string, string>>> GetServerMetadataDelegate { get; set; }

        public Func<string, IDictionary<string, string>, Task> UpdateServerMetadataDelegate { get; set; }

        public Func<string, string, Task> DeleteServerMetadataDelegate { get; set; }

        public Func<string, string, Task> AssignFloatingIpDelegate { get; set; }

        public Func<string, Task<ComputeServer>> GetServerDelegate { get; set; }

        public Func<Task<IEnumerable<ComputeServer>>> GetServersDelegate { get; set; }

        public Func<string, Task<ComputeKeyPair>> GetKeyPairDelegate { get; set; }

        public Func<Task<IEnumerable<ComputeKeyPair>>> GetKeyPairsDelegate { get; set; } 

        public async Task<ComputeFlavor> GetFlavor(string flavorId)
        {
            return await this.GetFlavorDelegate(flavorId);
        }

        public async Task<IDictionary<string, string>> GetServerMetadata(string serverId)
        {
            return await this.GetServerMetadataDelegate(serverId);
        }

        public async Task UpdateServerMetadata(string serverId, IDictionary<string, string> metadata)
        {
            await this.UpdateServerMetadataDelegate(serverId, metadata);
        }

        public async Task DeleteServerMetadata(string serverId, string key)
        {
            await this.DeleteServerMetadataDelegate(serverId, key);
        }

        public async Task<IEnumerable<ComputeKeyPair>> GetKeyPairs()
        {
             return await this.GetKeyPairsDelegate();
        }

        public async Task<ComputeKeyPair> GetKeyPair(string keyPairName)
        {
            return await this.GetKeyPairDelegate(keyPairName);
        }

        public async Task<IEnumerable<ComputeImage>> GetImages()
        {
            return await this.GetImagesDelegate();
        }

        public async Task<ComputeImage> GetImage(string imageId)
        {
            return await this.GetImageDelegate(imageId);
        }

        public async Task DeleteImage(string imageId)
        {
            await this.DeleteImageDelegate(imageId);
        }

        public async Task<ComputeServer> CreateServer(string name, string imageId, string flavorId, string networkId, string keyName, IEnumerable<string> securityGroups)
        {
            return await this.CreateServerDelegate(name, imageId, flavorId, networkId, keyName, securityGroups);
        }

        public async Task DeleteServer(string serverId)
        {
            await this.DeleteServerDelegate(serverId);
        }

        public async Task<IEnumerable<ComputeServer>> GetServers()
        {
            return await this.GetServersDelegate();
        }

        public async Task<ComputeServer> GetServer(string serverId)
        {
            return await this.GetServerDelegate(serverId);
        }

        public async Task AssignFloatingIp(string serverId, string ipAddress)
        {
            await this.AssignFloatingIpDelegate(serverId, ipAddress);
        }

        public async Task<IDictionary<string, string>> GetImageMetadata(string flavorId)
        {
            return await this.GetImageMetadataDelegate(flavorId);
        }

        public async Task UpdateImageMetadata(string flavorId, IDictionary<string, string> metadata)
        {
            await this.UpdateImageMetadataDelegate(flavorId, metadata);
        }

        public async Task DeleteImageMetadata(string flavorId, string key)
        {
            await this.DeleteImageMetadataDelegate(flavorId, key);
        }

        public async Task<IEnumerable<ComputeFlavor>> GetFlavors()
        {
            return await this.GetFlavorsDelegate();
        }
    }

    public class TestComputeServicePocoClientFactory : IComputeServicePocoClientFactory
    {
        internal IComputeServicePocoClient client;

        public TestComputeServicePocoClientFactory(IComputeServicePocoClient client)
        {
            this.client = client;
        }

        public IComputeServicePocoClient Create(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            return client;
        }
    }
}
