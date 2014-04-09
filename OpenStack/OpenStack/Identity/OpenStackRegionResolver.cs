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

            var identService = catalog.FirstOrDefault(s => string.Compare(s.Name, serviceName, StringComparison.Ordinal) == 0);
            
            if (identService == null)
            {
                return ret;
            }

            var defaultRegionEndpoint = identService.Endpoints.FirstOrDefault(e => endpoint.AbsoluteUri.Contains(e.PublicUri));
            if (defaultRegionEndpoint != null && !string.IsNullOrEmpty(defaultRegionEndpoint.Region))
            {
                ret = defaultRegionEndpoint.Region;
            }
            return ret;
        }
    }
}
