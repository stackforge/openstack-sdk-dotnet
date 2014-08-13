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
using System.Threading.Tasks;

namespace OpenStack.Network
{
    /// <summary>
    /// Client that can interact with an OpenStack network service.
    /// </summary>
    public interface INetworkServicePocoClient
    {
        /// <summary>
        /// Gets a list of networks available on the remote OpenStack instance.
        /// </summary>
        /// <returns>An enumerable list of networks.</returns>
        Task<IEnumerable<Network>> GetNetworks();

        /// <summary>
        /// Gets a list of Floating IPs from the remote OpenStack instance.
        /// </summary>
        /// <returns>An enumerable list of floating ips.</returns>
        Task<IEnumerable<FloatingIp>> GetFloatingIps();

        /// <summary>
        /// Gets the details of a Floating IP from the remote OpenStack instance.
        /// </summary>
        /// <param name="floatingIpId">The id of the target floating ip.</param>
        /// <returns>A FloatingIp object.</returns>
        Task<FloatingIp> GetFloatingIp(string floatingIpId);

        /// <summary>
        /// Creates a Floating IP on the remote OpenStack instance.
        /// </summary>
        /// <param name="networkId">The network id to use when creating the new ip address.</param>
        /// <returns>A FloatingIp object.</returns>
        Task<FloatingIp> CreateFloatingIp(string networkId);

        /// <summary>
        /// Deletes a Floating IP on the remote OpenStack instance.
        /// </summary>
        /// <param name="floatingIpId">The id of he floating ip to delete.</param>
        /// <returns>An async task.</returns>
        Task DeleteFloatingIp(string floatingIpId);
    }
}
