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
    using System.Security;
    using Openstack.Common;

    /// <inheritdoc/>
    public class OpenstackCredential : IOpenstackCredential
    {
        /// <inheritdoc/>
        public Uri AuthenticationEndpoint { get; private set; }

        /// <inheritdoc/>
        public string AccessTokenId { get; private set; }

        /// <inheritdoc/>
        public string UserName { get; private set; }

        /// <inheritdoc/>
        public SecureString Password { get; private set; }

        /// <inheritdoc/>
        public string TenantId { get; private set; }

        /// <inheritdoc/>
        public string Region { get; private set; }

        /// <inheritdoc/>
        public OpenstackServiceCatalog ServiceCatalog { get; private set; }

        /// <summary>
        /// Creates a new instance of the OpenstackCredential class.
        /// </summary>
        /// <param name="endpoint">The endpoint to be used for authentication.</param>
        /// <param name="userName">the user name to be used for authentication.</param>
        /// <param name="password">The password to be used for authentication.</param>
        /// <param name="tenantId">The tenant id to be used for the authentication.</param>
        /// <param name="region">The region to be used for the authentication.</param>
        public OpenstackCredential(Uri endpoint, string userName, SecureString password, string tenantId)
        {
            endpoint.AssertIsNotNull("endpoint", "An Openstack credential cannot be created with a null endpoint.");
            userName.AssertIsNotNullOrEmpty("userName", "An Openstack credential cannot be created with a null or empty user name.");
            password.AssertIsNotNull("password", "An Openstack credential cannot be created with a null password.");
            tenantId.AssertIsNotNullOrEmpty("tenantId", "An Openstack credential cannot be created with a null or empty tenant id.");

            this.Init(endpoint, userName, password, tenantId, string.Empty);
        }

        /// <summary>
        /// Creates a new instance of the OpenstackCredential class.
        /// </summary>
        /// <param name="endpoint">The endpoint to be used for authentication.</param>
        /// <param name="userName">the user name to be used for authentication.</param>
        /// <param name="password">The password to be used for authentication.</param>
        /// <param name="tenantId">The tenant id to be used for the authentication.</param>
        /// <param name="region">The region to be used for the authentication.</param>
        public OpenstackCredential(Uri endpoint, string userName, SecureString password, string tenantId, string region)
        {
            endpoint.AssertIsNotNull("endpoint","An Openstack credential cannot be created with a null endpoint.");
            userName.AssertIsNotNullOrEmpty("userName", "An Openstack credential cannot be created with a null or empty user name.");
            password.AssertIsNotNull("password", "An Openstack credential cannot be created with a null password.");
            tenantId.AssertIsNotNullOrEmpty("tenantId", "An Openstack credential cannot be created with a null or empty tenant id.");
            region.AssertIsNotNullOrEmpty("region", "An Openstack credential cannot be created with a null or empty region.");

            this.Init(endpoint, userName, password, tenantId, region);
        }

        internal void Init(Uri endpoint, string userName, SecureString password, string tenantId, string region)
        {
            this.AuthenticationEndpoint = endpoint;
            this.UserName = userName;
            this.Password = password;
            this.TenantId = tenantId;
            this.Region = region;

            this.ServiceCatalog = new OpenstackServiceCatalog();
        }

        /// <inheritdoc/>
        public void SetAccessTokenId(string accessTokenId)
        {
            accessTokenId.AssertIsNotNullOrEmpty("accessTokenId","Access token cannot be null or empty.");
            this.AccessTokenId = accessTokenId;
        }

        public void SetRegion(string region)
        {
            region.AssertIsNotNullOrEmpty("region", "Cannot set the region for this credential. Region cannot be null or empty.");
            this.Region = region;
        }

        /// <inheritdoc/>
        public void SetServiceCatalog(OpenstackServiceCatalog catalog)
        {
            catalog.AssertIsNotNull("catalog", "Service catalog cannot be null or empty.");
            this.ServiceCatalog = catalog;
        }
    }
}
