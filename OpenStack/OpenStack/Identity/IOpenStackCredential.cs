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

using System.Security;

namespace OpenStack.Identity
{
    /// <inheritdoc/>
    public interface IOpenStackCredential : ICredential
    {
        /// <summary>
        /// Gets the name of the user to use for the current instance of OpenStack 
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the password to use for the current instance of OpenStack 
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the Id of the tenant to use for the current instance of OpenStack 
        /// </summary>
        string TenantId { get; }

        /// <summary>
        /// Gets the current region for this credential.
        /// </summary>
        string Region { get; }

        /// <summary>
        /// Sets the access token to be used for the current instance of OpenStack.
        /// </summary>
        /// <param name="accessTokenId">The access token id.</param>
        void SetAccessTokenId(string accessTokenId);

        /// <summary>
        /// Sets the current region to use for the current instance of OpenStack.
        /// </summary>
        /// <param name="region">The region.</param>
        void SetRegion(string region);

        /// <summary>
        /// Sets the service catalog to be used for the current instance of OpenStack.
        /// </summary>
        /// <param name="catalog">The service catalog.</param>
        void SetServiceCatalog(OpenStackServiceCatalog catalog);
    }
}
