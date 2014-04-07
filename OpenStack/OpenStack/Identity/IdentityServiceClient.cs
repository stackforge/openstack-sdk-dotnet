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
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Identity
{
    /// <inheritdoc/>
    internal class IdentityServiceClient : IIdentityServiceClient
    {
        internal IOpenStackCredential Credential;
        internal CancellationToken CancellationToken;

        /// <summary>
        /// Creates a new instance of the IdentityServiceClient class.
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="cancellationToken"></param>
        internal IdentityServiceClient(IOpenStackCredential credential, CancellationToken cancellationToken)
        {
            this.Credential = credential;
            this.CancellationToken = cancellationToken;
        }

        /// <inheritdoc/>
        public async Task<IOpenStackCredential> Authenticate()
        {
            var client = ServiceLocator.Instance.Locate<IIdentityServicePocoClientFactory>().Create(this.Credential, this.CancellationToken);
            this.Credential = await client.Authenticate();
            return this.Credential;
        }
    }
}
