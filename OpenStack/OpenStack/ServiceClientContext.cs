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
using System.Threading;
using OpenStack.Identity;

namespace OpenStack
{
    /// <summary>
    /// Wrapper class that provides a context for the various storage clients.
    /// </summary>
    public class ServiceClientContext
    {
        /// <summary>
        /// Gets or sets a credential that can be used to connect to the remote OpenStack service.
        /// </summary>
        public IOpenStackCredential Credential { get; set; }

        /// <summary>
        /// Gets or sets a cancellation token that can be used when connecting to the remote OpenStack service.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the name of the storage service.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the public endpoint.
        /// </summary>
        public Uri PublicEndpoint { get; set; }

        /// <summary>
        /// Creates a new instance of the ServiceClientContext class.
        /// </summary>
        /// <param name="credential">The credential for this context.</param>
        /// <param name="cancellationToken">The cancellation token for this context.</param>
        /// <param name="serviceName">The name of the storage service.</param>
        /// <param name="publicEndpoint">The Uri for the public endpoint of the storage service.</param>
        internal ServiceClientContext(IOpenStackCredential credential, CancellationToken cancellationToken, string serviceName, Uri publicEndpoint)
        {
            this.Credential = credential;
            this.CancellationToken = cancellationToken;
            this.ServiceName = serviceName;
            this.PublicEndpoint = publicEndpoint;
        }
    }
}
