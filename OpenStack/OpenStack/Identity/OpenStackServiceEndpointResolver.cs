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
using System.Linq;
using OpenStack.Common;

namespace OpenStack.Identity
{
    /// <inheritdoc/>
    internal class OpenStackServiceEndpointResolver : IOpenStackServiceEndpointResolver
    {
        /// <inheritdoc/>
        public string ResolveEndpoint(ICollection<OpenStackServiceDefinition> catalog, string serviceName, string region)
        {
            catalog.AssertIsNotNull("catalog","Cannot resolve the public endpoint of a service with a null catalog.");
            serviceName.AssertIsNotNullOrEmpty("serviceName", "Cannot resolve the public endpoint of a service with a null or empty service name.");
            serviceName.AssertIsNotNullOrEmpty("region", "Cannot resolve the public endpoint of a service with a null or empty region.");

            if (catalog.All(s => !string.Equals(s.Name, serviceName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(string.Format("Service catalog does not contain an entry for the '{0}' service. The request could not be completed.", serviceName));
            }

            var service = catalog.First(s => string.Equals(s.Name, serviceName, StringComparison.OrdinalIgnoreCase));

            if (service.Endpoints.All(e => !string.Equals(e.Region, region, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(string.Format("Service catalog does not contain an endpoint for the '{0}' service in the requested region. Region: '{1}'", serviceName, region));
            }

            var endpoint = service.Endpoints.First(e => string.Equals(e.Region, region, StringComparison.OrdinalIgnoreCase));
            
            return endpoint.PublicUri;
        }
    }
}
