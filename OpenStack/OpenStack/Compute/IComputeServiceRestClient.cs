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

using System.Collections.Generic;

namespace OpenStack.Compute
{
    using System.Threading.Tasks;
    using OpenStack.Common.Http;

    /// <summary>
    /// Client that can connect to the REST endpoints of an OpenStack Compute Service
    /// </summary>
    public interface IComputeServiceRestClient
    {
        /// <summary>
        /// Gets a list of compute Flavors from the remote OpenStack instance.
        /// </summary>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetFlavors();

        /// <summary>
        /// Gets the detailed metadata for a compute flavor.
        /// </summary>
        /// <param name="flavorId">The id of the flavor.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetFlavor(string flavorId);

        /// <summary>
        /// Gets a list of compute images from the remote OpenStack instance.
        /// </summary>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetImages();

        /// <summary>
        /// Gets the detailed info for a compute image.
        /// </summary>
        /// <param name="imageId">The id of the image.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetImage(string imageId);

        /// <summary>
        /// Deletes a compute image.
        /// </summary>
        /// <param name="imageId">The id of the image.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> DeleteImage(string imageId);

        /// <summary>
        /// Gets the associated metadata for a given compute image.
        /// </summary>
        /// <param name="imageId">The id for the image.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetImageMetadata(string imageId);

        /// <summary>
        /// Updates the metadata for a given compute image. 
        /// Note: If a key does not exist on the remote server, it will be created.
        /// </summary>
        /// <param name="imageId">The id for the image.</param>
        /// <param name="metadata">A collection of key value pairs.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> UpdateImageMetadata(string imageId, IDictionary<string, string> metadata);

        /// <summary>
        /// Deletes the given key from the metadata for the given compute image.
        /// </summary>
        /// <param name="imageId">The id for the image.</param>
        /// <param name="key">The metadata key to remove.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> DeleteImageMetadata(string imageId, string key);

        /// <summary>
        /// Creates a new server on the remote OpenStack instance.
        /// </summary>
        /// <param name="name">The name of the server.</param>
        /// <param name="imageId">The id for the image that this server will be based on.</param>
        /// <param name="flavorId">The id of the flavor to use for this server.</param>
        /// <param name="networkId">The network to connect this server to.</param>
        /// <param name="keyName">The name of the key pair to associate with this server.</param>
        /// <param name="securityGroups">A list of security group names to associate with this server.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> CreateServer(string name, string imageId, string flavorId, string networkId, string keyName, IEnumerable<string> securityGroups);

        /// <summary>
        /// Gets a list of servers from the remote OpenStack instance.
        /// </summary>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetServers();

        /// <summary>
        /// Gets a server from the remote OpenStack instance.
        /// </summary>
        /// <param name="serverId">The id of the server to get.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetServer(string serverId);

        /// <summary>
        /// Deletes a server on the remote OpenStack instance.
        /// </summary>
        /// <param name="serverId">The id of the server to delete.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> DeleteServer(string serverId);

        /// <summary>
        /// Assigns the given floating ip address to the specified compute server.
        /// </summary>
        /// <param name="serverId">The id for the compute server.</param>
        /// <param name="ipAddress">The ip address of the floating ip.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> AssignFloatingIp(string serverId, string ipAddress);

        /// <summary>
        /// Gets the associated metadata for a given compute flavor.
        /// </summary>
        /// <param name="flavorId">The id for the flavor.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetServerMetadata(string flavorId);

        /// <summary>
        /// Updates the metadata for a given compute flavor. 
        /// Note: If a key does not exist on the remote server, it will be created.
        /// </summary>
        /// <param name="serverId">The id for the flavor.</param>
        /// <param name="metadata">A collection of key value pairs.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> UpdateServerMetadata(string serverId, IDictionary<string, string> metadata);

        /// <summary>
        /// Deletes the given key from the metadata for the given compute flavor.
        /// </summary>
        /// <param name="serverId">The id for the flavor</param>
        /// <param name="key">The metadata key to remove.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> DeleteServerMetadata(string serverId, string key);

        /// <summary>
        /// Gets a list of key pairs from the remote OpenStack instance.
        /// </summary>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetKeyPairs();

        /// <summary>
        /// Gets a key pair from the remote OpenStack instance.
        /// </summary>
        /// <param name="keyPairName">The name of the key pair to get.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetKeyPair(string keyPairName);
    }
}
