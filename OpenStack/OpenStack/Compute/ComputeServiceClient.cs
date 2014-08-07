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
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Compute
{
    /// <inheritdoc/>
    internal class ComputeServiceClient : IComputeServiceClient
    {
        internal ServiceClientContext Context;
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the ComputeServiceClient class.
        /// </summary>
        /// <param name="credentials">The credential to be used by this client.</param>
        /// <param name="token">The cancellation token to be used by this client.</param>
        /// <param name="serviceName">The name of the service to be used by this client.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        public ComputeServiceClient(IOpenStackCredential credentials, string serviceName, CancellationToken token, IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a storage service client with a null service locator.");

            this.ServiceLocator = serviceLocator;
            var endpoint = new Uri(credentials.ServiceCatalog.GetPublicEndpoint(serviceName, credentials.Region));
            this.Context = new ServiceClientContext(credentials, token, serviceName, endpoint);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ComputeFlavor>> GetFlavors()
        {
            var client = this.GetPocoClient();
            return await client.GetFlavors();
        }

        /// <inheritdoc/>
        public async Task<ComputeFlavor> GetFlavor(string flavorId)
        {
            flavorId.AssertIsNotNullOrEmpty("flavorId", "Cannot get a compute flavor with a null or empty id.");
            
            var client = this.GetPocoClient();
            return await client.GetFlavor(flavorId);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, string>> GetServerMetadata(string flavorId)
        {
            flavorId.AssertIsNotNullOrEmpty("flavorId", "Cannot get compute flavor metadata with a null or empty id.");
            
            var client = this.GetPocoClient();
            return await client.GetServerMetadata(flavorId);
        }

        /// <inheritdoc/>
        public async Task UpdateServerMetadata(string flavorId, IDictionary<string, string> metadata)
        {
            flavorId.AssertIsNotNullOrEmpty("flavorId", "Cannot update compute flavor metadata with a null or empty id.");
            metadata.AssertIsNotNull("metadata", "Cannot update compute flavor metadata with a null or empty metadata.");

            var client = this.GetPocoClient();
            await client.UpdateServerMetadata(flavorId, metadata);
        }

        /// <inheritdoc/>
        public async Task DeleteServerMetadata(string flavorId, string key)
        {
            flavorId.AssertIsNotNullOrEmpty("flavorId", "Cannot delete a compute flavor metadata item with a null or empty id.");
            key.AssertIsNotNullOrEmpty("key", "Cannot delete a compute flavor metadata item with a null or empty key.");

            var client = this.GetPocoClient();
            await client.DeleteServerMetadata(flavorId, key);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ComputeImage>> GetImages()
        {
            var client = this.GetPocoClient();
            return await client.GetImages();
        }

        /// <inheritdoc/>
        public async Task<ComputeImage> GetImage(string imageId)
        {
            imageId.AssertIsNotNullOrEmpty("imageId", "Cannot get a compute image with a null or empty id.");

            var client = this.GetPocoClient();
            return await client.GetImage(imageId);
        }

        /// <inheritdoc/>
        public async Task DeleteImage(string imageId)
        {
            imageId.AssertIsNotNullOrEmpty("imageId", "Cannot delete a compute image with a null or empty id.");

            var client = this.GetPocoClient();
            await client.DeleteImage(imageId);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, string>> GetImageMetadata(string imageId)
        {
            imageId.AssertIsNotNullOrEmpty("flavorId", "Cannot get compute image metadata with a null or empty id.");

            var client = this.GetPocoClient();
            return await client.GetImageMetadata(imageId);
        }

        /// <inheritdoc/>
        public async Task UpdateImageMetadata(string imageId, IDictionary<string, string> metadata)
        {
            imageId.AssertIsNotNullOrEmpty("imageId", "Cannot update compute image metadata with a null or empty id.");
            metadata.AssertIsNotNull("metadata", "Cannot update compute image metadata with a null or empty metadata.");

            var client = this.GetPocoClient();
            await client.UpdateImageMetadata(imageId, metadata);
        }

        /// <inheritdoc/>
        public async Task DeleteImageMetadata(string imageId, string key)
        {
            imageId.AssertIsNotNullOrEmpty("imageId", "Cannot delete a compute image metadata item with a null or empty id.");
            key.AssertIsNotNullOrEmpty("key", "Cannot delete a compute image metadata item with a null or empty key.");

            var client = this.GetPocoClient();
            await client.DeleteImageMetadata(imageId, key);
        }

        /// <summary>
        /// Gets a client to interact with the remote OpenStack instance.
        /// </summary>
        /// <returns>A POCO client.</returns>
        internal IComputeServicePocoClient GetPocoClient()
        {
            return this.ServiceLocator.Locate<IComputeServicePocoClientFactory>().Create(this.Context, this.ServiceLocator);
        }
    }
}
