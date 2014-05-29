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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Identity
{
    /// <inheritdoc/>
    internal class IdentityServiceRestClient : IIdentityServiceRestClient
    {
        internal IOpenStackCredential Credential;
        internal CancellationToken CancellationToken;
        internal IServiceLocator ServiceLocator;
        
        /// <summary>
        /// Creates a new instance of the IdentityServiceRestClient class.
        /// </summary>
        /// <param name="credential">The credential to be used by this client.</param>
        /// <param name="cancellationToken">The cancellation token to be used by this client.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        public IdentityServiceRestClient(IOpenStackCredential credential, CancellationToken cancellationToken, IServiceLocator serviceLocator)
        {
            credential.AssertIsNotNull("credential");
            cancellationToken.AssertIsNotNull("cancellationToken");
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create an identity service rest client with a null service locator.");

            this.Credential = credential;
            this.CancellationToken = cancellationToken;
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> Authenticate()
        {
            var client = this.ServiceLocator.Locate<IHttpAbstractionClientFactory>().Create(this.CancellationToken);
            client.Headers.Add("Accept", "application/json");
            client.ContentType = "application/json";

            client.Uri = new Uri(string.Format("{0}/tokens", this.Credential.AuthenticationEndpoint));
            client.Method = HttpMethod.Post;
            client.Content = CreateAuthenticationJsonPayload(this.Credential).ConvertToStream();

            return await client.SendAsync();
        }

        /// <summary>
        /// Creates a Json payload that will be sent to the remote instance to authenticate.
        /// </summary>
        /// <param name="creds">The credentials used to authenticate.</param>
        /// <returns>A string that represents a Json payload.</returns>
        internal static string CreateAuthenticationJsonPayload(IOpenStackCredential creds)
        {
            var authPayload = new StringBuilder();
            authPayload.Append("{\"auth\":{\"passwordCredentials\":{\"username\":\"");
            authPayload.Append(creds.UserName);
            authPayload.Append("\",\"password\":\"");
            authPayload.Append(creds.Password);
            authPayload.Append("\"},\"tenantName\":\"");
            authPayload.Append(creds.TenantId);
            authPayload.Append("\"}}");
            return authPayload.ToString();
        }
    }
}
