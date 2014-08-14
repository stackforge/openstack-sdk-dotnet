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

namespace OpenStack.Compute
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Client that can interact with an OpenStack compute service.
    /// </summary>
    public interface IComputeServicePocoClient
    {
        /// <summary>
        /// Gets a list of flavors that are available on the remote OpenStack instance.
        /// </summary>
        /// <returns>An enumerable list of Flavors.</returns>
        Task<IEnumerable<ComputeFlavor>> GetFlavors();

        /// <summary>
        /// Gets the detailed metadata for a compute flavor.
        /// </summary>
        /// <param name="flavorId">The id of the flavor.</param>
        /// <returns>An object representing a compute flavor.</returns>
        Task<ComputeFlavor> GetFlavor(string flavorId);

        /// <summary>
        /// Gets a list of images that are available on the remote OpenStack instance.
        /// </summary>
        /// <returns>An enumerable list of images.</returns>
        Task<IEnumerable<ComputeImage>> GetImages();

        /// <summary>
        /// Gets the detailed metadata for a compute image.
        /// </summary>
        /// <param name="imageId">The id of the image.</param>
        /// <returns>An object representing a compute image.</returns>
        Task<ComputeImage> GetImage(string imageId);

        /// <summary>
        /// Deletes the image with the given id from the remote OpenStack instance.
        /// </summary>
        /// <param name="imageId">The id of the image.</param>
        /// <returns>An async task.</returns>
        Task DeleteImage(string imageId);

        /// <summary>
        /// Creates a new server on the remote OpenStack instance.
        /// </summary>
        /// <param name="name">The name of the server.</param>
        /// <param name="imageId">The id for the image that this server will be based on.</param>
        /// <param name="flavorId">The id of the flavor to use for this server.</param>
        /// <param name="networkId">The network to connect this server to.</param>
        /// <param name="keyName">The name of the key to associate with this server.</param>
        /// <param name="securityGroups">A list of security group names to associate with this server.</param>
        /// <returns>A server object.</returns>
        Task<ComputeServer> CreateServer(string name, string imageId, string flavorId, string networkId, string keyName, IEnumerable<string> securityGroups);

        /// <summary>
        /// Deletes the server with the given id from the remote OpenStack instance.
        /// </summary>
        /// <param name="serverId">The id of the server.</param>
        /// <returns>An async task.</returns>
        Task DeleteServer(string serverId);

        /// <summary>
        /// Gets a list of servers that are available on the remote OpenStack instance.
        /// </summary>
        /// <returns>An enumerable list of servers.</returns>
        Task<IEnumerable<ComputeServer>> GetServers();

        /// <summary>
        /// Get the server with the given id from the remote OpenStack instance.
        /// </summary>
        /// <param name="serverId">The id of the server.</param>
        /// <returns>An async task.</returns>
        Task<ComputeServer> GetServer(string serverId);

        /// <summary>
        /// Assigns a floating ip address to a compute server on the remote OpenStack instance.
        /// </summary>
        /// <param name="serverId">The id of the server.</param>
        /// <param name="ipAddress">The ip address of the floating ip to assign.</param>
        /// <returns>An async task.</returns>
        Task AssignFloatingIp(string serverId, string ipAddress);

        /// <summary>
        /// Gets the associated metadata for a given compute image.
        /// </summary>
        /// <param name="imageId">The id for the image.</param>
        /// <returns>A collection of key values pairs.</returns>
        Task<IDictionary<string, string>> GetImageMetadata(string imageId);

        /// <summary>
        /// Updates the metadata for a given compute image. 
        /// Note: If a key does not exist on the remote server, it will be created.
        /// </summary>
        /// <param name="imageId">The id for the image.</param>
        /// <param name="metadata">A collection of key value pairs.</param>
        /// <returns>An async task.</returns>
        Task UpdateImageMetadata(string imageId, IDictionary<string, string> metadata);

        /// <summary>
        /// Deletes the given key from the metadata for the given compute image.
        /// </summary>
        /// <param name="imageId">The id for the image</param>
        /// <param name="key">The metadata key to remove.</param>
        /// <returns>An async task.</returns>
        Task DeleteImageMetadata(string imageId, string key);

        /// <summary>
        /// Gets the associated metadata for a given compute server.
        /// </summary>
        /// <param name="serverId">The id for the server.</param>
        /// <returns>A collection of key values pairs.</returns>
        Task<IDictionary<string, string>> GetServerMetadata(string serverId);

        /// <summary>
        /// Updates the metadata for a given compute server. 
        /// Note: If a key does not exist on the remote server, it will be created.
        /// </summary>
        /// <param name="serverId">The id for the server.</param>
        /// <param name="metadata">A collection of key value pairs.</param>
        /// <returns>An async task.</returns>
        Task UpdateServerMetadata(string serverId, IDictionary<string, string> metadata);

        /// <summary>
        /// Deletes the given key from the metadata for the given compute server.
        /// </summary>
        /// <param name="serverId">The id for the server</param>
        /// <param name="key">The metadata key to remove.</param>
        /// <returns>An async task.</returns>
        Task DeleteServerMetadata(string serverId, string key);

        /// <summary>
        /// Gets a list of key pairs that are available on the remote OpenStack instance.
        /// </summary>
        /// <returns>An enumerable list of key pairs.</returns>
        Task<IEnumerable<ComputeKeyPair>> GetKeyPairs();

        /// <summary>
        /// Gets the key pair with the given name from the remote OpenStack instance.
        /// </summary>
        /// <param name="keyPairName">The name of the key pair.</param>
        /// <returns>An async task.</returns>
        Task<ComputeKeyPair> GetKeyPair(string keyPairName);
    }
}
