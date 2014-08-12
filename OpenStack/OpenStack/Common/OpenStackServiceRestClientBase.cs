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
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Common
{
    /// <summary>
    /// Base class for OpenStack service clients
    /// </summary>
    internal abstract class OpenStackServiceRestClientBase : IOpenStackServiceClient
    {
        internal ServiceClientContext Context;
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the OpenStackServiceRestClientBase class.
        /// </summary>
        /// <param name="context">The service client context for this object.</param>
        /// <param name="serviceLocator">A service locator for this object to use.</param>
        protected OpenStackServiceRestClientBase(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a service rest client with a null service locator.");

            this.ServiceLocator = serviceLocator;
            this.Context = context;
        }

        /// <summary>
        /// Creates an Http client to communicate with the remote OpenStack service.
        /// </summary>
        /// <param name="context">The compute context to use when creating the client.</param>
        /// <returns>The Http client.</returns>
        internal IHttpAbstractionClient GetHttpClient(ServiceClientContext context)
        {
            var client = this.ServiceLocator.Locate<IHttpAbstractionClientFactory>().Create(context.CancellationToken);
            AddAuthenticationHeader(context.Credential.AccessTokenId, client);
            client.Headers.Add("Accept", "application/json");
            return client;
        }

        /// <summary>
        /// Creates a Uri for making requests to the remote service.
        /// </summary>
        /// <param name="endpoint">The root endpoint to use in the request.</param>
        /// <param name="values">The additional parameters to add to the request.</param>
        /// <returns>A complete request Uri.</returns>
        internal Uri CreateRequestUri(Uri endpoint, params string[] values)
        {
            var temp = new List<string> { endpoint.AbsoluteUri.TrimEnd('/') };
            temp.AddRange(values);
            return new Uri(string.Join("/", temp.ToArray()));
        }

        /// <summary>
        /// Adds the appropriate authentication headers to the given http client.
        /// </summary>
        /// <param name="authenticationId">The access token id for the header.</param>
        /// <param name="client">The http client.</param>
        internal void AddAuthenticationHeader(string authenticationId, IHttpAbstractionClient client)
        {
            client.Headers.Add("X-Auth-Token", authenticationId);
        }
    }
}
