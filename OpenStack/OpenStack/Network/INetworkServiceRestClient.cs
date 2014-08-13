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

using System.Threading.Tasks;
using OpenStack.Common.Http;

namespace OpenStack.Network
{
    /// <summary>
    /// Client that can connect to the REST endpoints of an OpenStack Network Service
    /// </summary>
    public interface INetworkServiceRestClient
    {
        /// <summary>
        /// Gets a list of Networks from the remote OpenStack instance.
        /// </summary>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetNetworks();

        /// <summary>
        /// Gets a list of Floating IPs from the remote OpenStack instance.
        /// </summary>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetFloatingIps();

        /// <summary>
        /// Gets the details of a Floating IP from the remote OpenStack instance.
        /// </summary>
        /// <param name="floatingIpId">The id of the target floating ip.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetFloatingIp(string floatingIpId);

        /// <summary>
        /// Creates a Floating IP on the remote OpenStack instance.
        /// </summary>
        /// <param name="networkId">The network id to use when creating the new ip address.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> CreateFloatingIp(string networkId);

        /// <summary>
        /// Deletes a Floating IP on the remote OpenStack instance.
        /// </summary>
        /// <param name="floatingIpId">The id of the floating ip to delete.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> DeleteFloatingIp(string floatingIpId);
    }
}
