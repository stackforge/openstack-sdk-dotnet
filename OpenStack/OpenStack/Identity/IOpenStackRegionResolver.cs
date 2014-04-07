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

namespace OpenStack.Identity
{
    /// <summary>
    /// Resolves a region/availability zone from a service catalog
    /// </summary>
    public interface IOpenStackRegionResolver
    {
        /// <summary>
        /// Resolves a region from the given service catalog using an endpoint uri and service name.
        /// </summary>
        /// <param name="endpoint">The endpoint to use when resolving the region.</param>
        /// <param name="catalog">The service catalog to search within.</param>
        /// <param name="serviceName">The service name to use when resolving the region.</param>
        /// <returns>The region if available, string.Empty if the region cannot be resolved.</returns>
        string Resolve(Uri endpoint, IEnumerable<OpenStackServiceDefinition> catalog, string serviceName);
    }
}
