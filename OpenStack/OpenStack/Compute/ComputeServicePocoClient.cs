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
using System.Net;
using System.Threading.Tasks;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Compute
{
    /// <inheritdoc/>
    internal class ComputeServicePocoClient : IComputeServicePocoClient
    {
        internal ServiceClientContext _context;
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the ComputeServicePocoClient class.
        /// </summary>
        /// <param name="context">The compute service context to use for this client.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        internal ComputeServicePocoClient(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a storage service poco client with a null service locator.");

            this._context = context;
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ComputeFlavor>> GetFlavors()
        {
            var client = this.GetRestClient();
            var resp = await client.GetFlavors();

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute flavors. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeFlavorPayloadConverter>();
            var flavors = converter.ConvertFlavors(await resp.ReadContentAsStringAsync());

            return flavors;
        }

        /// <inheritdoc/>
        public async Task<ComputeFlavor> GetFlavor(string flavorId)
        {
            var client = this.GetRestClient();
            var resp = await client.GetFlavor(flavorId);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute flavor. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeFlavorPayloadConverter>();
            var flavor = converter.ConvertFlavor(await resp.ReadContentAsStringAsync());

            return flavor;
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, string>> GetServerMetadata(string flavorId)
        {
            var client = this.GetRestClient();
            var resp = await client.GetServerMetadata(flavorId);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute server metadata. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeItemMetadataPayloadConverter>();
            var metadata = converter.Convert(await resp.ReadContentAsStringAsync());

            return metadata;
        }

        /// <inheritdoc/>
        public async Task UpdateServerMetadata(string flavorId, IDictionary<string, string> metadata)
        {
            var client = this.GetRestClient();
            var resp = await client.UpdateServerMetadata(flavorId, metadata);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to update compute server metadata. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task DeleteServerMetadata(string flavorId, string key)
        {
            var client = this.GetRestClient();
            var resp = await client.DeleteServerMetadata(flavorId, key);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to delete compute server metadata. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ComputeKeyPair>> GetKeyPairs()
        {
            var client = this.GetRestClient();
            var resp = await client.GetKeyPairs();

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute key pairs. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeKeyPairPayloadConverter>();
            var pairs = converter.ConvertKeyPairs(await resp.ReadContentAsStringAsync());

            return pairs;
        }

        /// <inheritdoc/>
        public async Task<ComputeKeyPair> GetKeyPair(string keyPairName)
        {
            var client = this.GetRestClient();
            var resp = await client.GetKeyPair(keyPairName);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute key pair. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeKeyPairPayloadConverter>();
            var keyPair = converter.Convert(await resp.ReadContentAsStringAsync());

            return keyPair;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ComputeImage>> GetImages()
        {
            var client = this.GetRestClient();
            var resp = await client.GetImages();

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute images. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeImagePayloadConverter>();
            var flavors = converter.ConvertImages(await resp.ReadContentAsStringAsync());

            return flavors;
        }

        /// <inheritdoc/>
        public async Task<ComputeImage> GetImage(string imageId)
        {
            var client = this.GetRestClient();
            var resp = await client.GetImage(imageId);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute image. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeImagePayloadConverter>();
            var image = converter.ConvertImage(await resp.ReadContentAsStringAsync());

            return image;
        }

        /// <inheritdoc/>
        public async Task DeleteImage(string imageId)
        {
            var client = this.GetRestClient();
            var resp = await client.DeleteImage(imageId);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to delete compute image. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task<ComputeServer> CreateServer(string name, string imageId, string flavorId, string networkId, string keyName,
            IEnumerable<string> securityGroups)
        {
            var client = this.GetRestClient();
            var resp = await client.CreateServer(name, imageId, flavorId, networkId, keyName, securityGroups);

            if (resp.StatusCode != HttpStatusCode.Accepted && resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to create compute server. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeServerPayloadConverter>();
            var metadata = converter.ConvertSummary(await resp.ReadContentAsStringAsync());

            return metadata;
        }

        /// <inheritdoc/>
        public async Task DeleteServer(string serverId)
        {
            var client = this.GetRestClient();
            var resp = await client.DeleteServer(serverId);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to delete compute server. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ComputeServer>> GetServers()
        {
            var client = this.GetRestClient();
            var resp = await client.GetServers();

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute images. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeServerPayloadConverter>();
            var servers = converter.ConvertServers(await resp.ReadContentAsStringAsync());

            return servers;
        }

        /// <inheritdoc/>
        public async Task<ComputeServer> GetServer(string serverId)
        {
            var client = this.GetRestClient();
            var resp = await client.GetServer(serverId);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute server. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeServerPayloadConverter>();
            var flavor = converter.Convert(await resp.ReadContentAsStringAsync());

            return flavor;
        }

        /// <inheritdoc/>
        public async Task AssignFloatingIp(string serverId, string ipAddress)
        {
            var client = this.GetRestClient();
            var resp = await client.AssignFloatingIp(serverId, ipAddress);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.Accepted)
            {
                throw new InvalidOperationException(string.Format("Failed to assign floating ip. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, string>> GetImageMetadata(string imageId)
        {
            var client = this.GetRestClient();
            var resp = await client.GetImageMetadata(imageId);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NonAuthoritativeInformation)
            {
                throw new InvalidOperationException(string.Format("Failed to get compute image metadata. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IComputeItemMetadataPayloadConverter>();
            var metadata = converter.Convert(await resp.ReadContentAsStringAsync());

            return metadata;
        }

        /// <inheritdoc/>
        public async Task UpdateImageMetadata(string imageId, IDictionary<string, string> metadata)
        {
            var client = this.GetRestClient();
            var resp = await client.UpdateImageMetadata(imageId, metadata);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to update compute image metadata. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }
        }

        /// <inheritdoc/>
        public async Task DeleteImageMetadata(string imageId, string key)
        {
            var client = this.GetRestClient();
            var resp = await client.DeleteImageMetadata(imageId, key);

            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(string.Format("Failed to delete compute image metadata. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }
        }

        /// <summary>
        /// Gets a client that can be used to connect to the REST endpoints of an OpenStack compute service.
        /// </summary>
        /// <returns>The client.</returns>
       internal IComputeServiceRestClient GetRestClient()
        {
            return this.ServiceLocator.Locate<IComputeServiceRestClientFactory>().Create(this._context, this.ServiceLocator);
        }
    }
}
