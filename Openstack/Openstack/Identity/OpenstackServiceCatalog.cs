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

namespace Openstack.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Openstack.Common.ServiceLocation;

    /// <inheritdoc/>
    public class OpenstackServiceCatalog : List<OpenstackServiceDefinition>, IOpenstackServiceCatalog
    {
        /// <inheritdoc/>
        public Uri GetPublicEndpoint(string serviceName, string region)
        {
            var resolver = ServiceLocator.Instance.Locate<IOpenstackServiceEndpointResolver>();
            return new Uri(resolver.ResolveEndpoint(this, serviceName, region));
        }

        /// <inheritdoc/>
        public bool Exists(string serviceName)
        {
            return this.Any(s => string.Compare(s.Name, serviceName, StringComparison.InvariantCulture) == 0);
        }

        /// <inheritdoc/>
        public IEnumerable<OpenstackServiceDefinition> GetServicesInAvailabilityZone(string availabilityZoneName)
        {
            return this.Where(s => s.Endpoints.Any(e => e.Region.Contains(availabilityZoneName)));
        }
    }
}
