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

using System.Threading;
using System.Threading.Tasks;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Identity
{
    /// <inheritdoc/>
    internal class IdentityServiceClient : IIdentityServiceClient
    {
        internal IOpenStackCredential Credential;
        internal CancellationToken CancellationToken;
        internal IServiceLocator ServiceLocator;
        internal string ServiceName;

        /// <summary>
        /// Creates a new instance of the IdentityServiceClient class.
        /// </summary>
        /// <param name="credential">The credential to be used by the client.</param>
        /// <param name="serviceName">The name of the service to be used by the client.</param>
        /// <param name="cancellationToken">A cancellation token to be used when completing requests.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        internal IdentityServiceClient(IOpenStackCredential credential, string serviceName, CancellationToken cancellationToken, IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create an identity service client with a null service locator.");
            
            this.ServiceLocator = serviceLocator;
            this.Credential = credential;
            this.CancellationToken = cancellationToken;
            this.ServiceName = serviceName;
        }

        /// <inheritdoc/>
        public async Task<IOpenStackCredential> Authenticate()
        {
            var client = this.ServiceLocator.Locate<IIdentityServicePocoClientFactory>().Create(this.Credential, this.ServiceName, this.CancellationToken, this.ServiceLocator);
            this.Credential = await client.Authenticate();
            return this.Credential;
        }
    }
}
