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
using System.Threading;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Identity
{
    /// <inheritdoc/>
    internal class IdentityServiceClientDefinition : IOpenStackServiceClientDefinition
    {
        internal const string DefaultServiceName = "Keystone";

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <summary>
        /// Creates a new instance of the IdentityServiceClientDefinition class.
        /// </summary>
        public IdentityServiceClientDefinition()
        {
            this.Name = typeof(IdentityServiceClient).Name;
        }

        /// <inheritdoc/>
        public IOpenStackServiceClient Create(ICredential credential, string serviceName, CancellationToken cancellationToken, IServiceLocator serviceLocator)
        {
            var srvName = string.IsNullOrEmpty(serviceName) ? DefaultServiceName : serviceName;
            return new IdentityServiceClient((IOpenStackCredential)credential, srvName, cancellationToken, serviceLocator);
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListSupportedVersions()
        {
            return new List<string>() { "2.0.0.0" };
        }

        /// <inheritdoc/>
        public bool IsSupported(ICredential credential, string serviceName)
        {
            if (credential != null && credential.AuthenticationEndpoint != null)
            {
                //https://someidentityendpoint:35357/v2.0
                var endpointSegs = credential.AuthenticationEndpoint.Segments;
                if (endpointSegs.Count() == 2 && string.Equals(endpointSegs[1].Trim('/'), "v2.0", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
