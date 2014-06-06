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

namespace OpenStack.Compute
{
    using System;
    using System.Threading.Tasks;
    using OpenStack.Common.Http;
    using OpenStack.Common;
    using OpenStack.Common.ServiceLocation;
    using System.Net.Http;

    /// <inheritdoc/>
    internal class ComputeServiceRestClient : OpenStackServiceRestClientBase, IComputeServiceRestClient
    {
        /// <summary>
        /// Creates a new instance of the ComputeServiceRestClient class.
        /// </summary>
        /// <param name="context">The current service client context to use.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        internal ComputeServiceRestClient(ServiceClientContext context, IServiceLocator serviceLocator) : base(context, serviceLocator)
        {
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetFlavors()
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint,"flavors");
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetFlavor(string flavorId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint,"flavors", flavorId);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetImages()
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, "images/detail");
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetImage(string imageId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, "images", imageId);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> DeleteImage(string imageId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, "images", imageId);
            client.Method = HttpMethod.Delete;

            return await client.SendAsync();
        }
    }
}
