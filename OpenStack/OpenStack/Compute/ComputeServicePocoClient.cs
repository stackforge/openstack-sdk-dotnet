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
