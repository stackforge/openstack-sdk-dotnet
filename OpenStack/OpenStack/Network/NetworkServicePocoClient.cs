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

namespace OpenStack.Network
{
    /// <inheritdoc/>
    class NetworkServicePocoClient : INetworkServicePocoClient
    {
        internal ServiceClientContext _context;
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the ComputeServicePocoClient class.
        /// </summary>
        /// <param name="context">The compute service context to use for this client.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        internal NetworkServicePocoClient(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a network service poco client with a null service locator.");

            this._context = context;
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Network>> GetNetworks()
        {
            var client = this.GetRestClient();
            var resp = await client.GetNetworks();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get networks. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<INetworkPayloadConverter>();
            var networks = converter.ConvertNetworks(await resp.ReadContentAsStringAsync());

            return networks;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FloatingIp>> GetFloatingIps()
        {
            var client = this.GetRestClient();
            var resp = await client.GetFloatingIps();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get floating ips. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IFloatingIpPayloadConverter>();
            var floatingIps = converter.ConvertFloatingIps(await resp.ReadContentAsStringAsync());

            return floatingIps;
        }

        /// <inheritdoc/>
        public async Task<FloatingIp> GetFloatingIp(string floatingIpId)
        {
            var client = this.GetRestClient();
            var resp = await client.GetFloatingIp(floatingIpId);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to get floating ip. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IFloatingIpPayloadConverter>();
            var floatingIp = converter.Convert(await resp.ReadContentAsStringAsync());

            return floatingIp;
        }

        /// <inheritdoc/>
        public async Task<FloatingIp> CreateFloatingIp(string networkId)
        {
            var client = this.GetRestClient();
            var resp = await client.CreateFloatingIp(networkId);

            if (resp.StatusCode != HttpStatusCode.Created && resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to create floating ip. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }

            var converter = this.ServiceLocator.Locate<IFloatingIpPayloadConverter>();
            var floatingIp = converter.Convert(await resp.ReadContentAsStringAsync());

            return floatingIp;
        }

        /// <inheritdoc/>
        public async Task DeleteFloatingIp(string floatingIpId)
        {
            var client = this.GetRestClient();
            var resp = await client.DeleteFloatingIp(floatingIpId);

            if (resp.StatusCode != HttpStatusCode.NoContent && resp.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format("Failed to delete floating ip. The remote server returned the following status code: '{0}'.", resp.StatusCode));
            }
        }

        /// <summary>
        /// Gets a client that can be used to connect to the REST endpoints of an OpenStack network service.
        /// </summary>
        /// <returns>The client.</returns>
        internal INetworkServiceRestClient GetRestClient()
        {
            return this.ServiceLocator.Locate<INetworkServiceRestClientFactory>().Create(this._context, this.ServiceLocator);
        }
    }
}
