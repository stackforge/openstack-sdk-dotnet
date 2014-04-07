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

using System.Collections.Generic;

namespace OpenStack.Identity
{
    /// <summary>
    /// Resolves services endpoints from a service catalog.
    /// </summary>
    public interface IOpenStackServiceEndpointResolver
    {
        /// <summary>
        /// Resolves a service endpoint from the supplied service catalog using the given service and region.
        /// </summary>
        /// <param name="catalog">The service catalog.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="region">The region of the endpoint.</param>
        /// <returns>a string that represents the resolved endpoint.</returns>
        string ResolveEndpoint(ICollection<OpenStackServiceDefinition> catalog, string serviceName, string region);
    }
}
