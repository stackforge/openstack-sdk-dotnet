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
using System.Linq;
using System.Threading;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Network
{
    /// <inheritdoc/>
    class NetworkServiceClientDefinition :IOpenStackServiceClientDefinition
    {
        internal const string DefaultServiceName = "Neutron";

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public IOpenStackServiceClient Create(ICredential credential, string serviceName, CancellationToken cancellationToken,
            IServiceLocator serviceLocator)
        {
            return new NetworkServiceClient((IOpenStackCredential)credential, GetServiceName(serviceName), cancellationToken, serviceLocator);
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListSupportedVersions()
        {
            return new List<string>() { "2.0", "2" };
        }

        /// <inheritdoc/>
        public bool IsSupported(ICredential credential, string serviceName)
        {
            if (credential == null || credential.ServiceCatalog == null)
            {
                return false;
            }

            var catalog = credential.ServiceCatalog;
            return
                catalog.Any(
                    s =>
                        string.Equals(s.Name, GetServiceName(serviceName), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the service name to use.
        /// </summary>
        /// <param name="serviceName">The given service name.</param>
        /// <returns>The given service name if it is not empty or null, otherwise the default service name will be returned.</returns>
        internal string GetServiceName(string serviceName)
        {
            return string.IsNullOrEmpty(serviceName) ? DefaultServiceName : serviceName;
        }
    }
}
