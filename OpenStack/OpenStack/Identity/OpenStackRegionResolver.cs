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
    internal class OpenStackRegionResolver : IOpenStackRegionResolver
    {
        public string Resolve(Uri endpoint, IEnumerable<OpenStackServiceDefinition> catalog, string serviceName)
        {
            endpoint.AssertIsNotNull("endpoint","Cannot resolve a region with a null endpoint.");
            catalog.AssertIsNotNull("catalog", "Cannot resolve a region with a null catalog.");
            serviceName.AssertIsNotNullOrEmpty("serviceName", "Cannot resolve a region with a null or empty service name.");

            var ret = string.Empty;
            var cat = catalog.ToList();

            var identService = cat.FirstOrDefault(s => string.Equals(s.Name, serviceName, StringComparison.OrdinalIgnoreCase));
            
            if (identService == null)
            {
                //Fall back and see if any service can be found that publishes an endpoint that matches the one given.
                identService = cat.FirstOrDefault(s => s.Endpoints.Any(e => endpoint.AbsoluteUri.TrimEnd('/').Contains(e.PublicUri.TrimEnd('/'))));
                if (identService == null)
                {
                    //if no service can be found, either by name or endpoint, then return string.empty
                    return ret;
                }
            }

            var defaultRegionEndpoint = identService.Endpoints.FirstOrDefault(e => endpoint.AbsoluteUri.TrimEnd('/').Contains(e.PublicUri.TrimEnd('/')));
            if (defaultRegionEndpoint != null && !string.IsNullOrEmpty(defaultRegionEndpoint.Region))
            {
                ret = defaultRegionEndpoint.Region;
            }

            return ret;
        }
    }
}
