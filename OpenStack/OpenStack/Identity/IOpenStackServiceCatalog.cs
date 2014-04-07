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
    /// A listing of services offered by a remote instance of OpenStack.
    /// </summary>
    internal interface IOpenStackServiceCatalog : IEnumerable<OpenStackServiceDefinition>
    {
        /// <summary>
        /// Gets the public endpoint for the given service and region.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="region">The region of the endpoint.</param>
        /// <returns>A Uri that represents the public endpoint for the given service in the given region.</returns>
        Uri GetPublicEndpoint(string serviceName, string region);

        /// <summary>
        /// Determines if the given service exists in the catalog.
        /// </summary>
        /// <param name="serviceName">The name of the service to check for.</param>
        /// <returns>A value indicating if the service could be found in the catalog.</returns>
        bool Exists(string serviceName);

        /// <summary>
        /// Gets a list of services available in the given region/availability zone.
        /// </summary>
        /// <param name="availabilityZoneName">The name of the region/availability zone.</param>
        /// <returns>A list of available services.</returns>
        IEnumerable<OpenStackServiceDefinition> GetServicesInAvailabilityZone(string availabilityZoneName);
    }
}