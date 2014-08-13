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

using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Network
{
    /// <inheritdoc/>
    internal class NetworkServiceRestClient : OpenStackServiceRestClientBase, INetworkServiceRestClient
    {
        internal const string NetworksUrlMoniker = "networks";
        internal const string FloatingIpsUrlMoniker = "floatingips";
        internal const string NetworkVersionMoniker = "v2.0";

        /// <summary>
        /// Creates a new instance of the NetworkServiceRestClient class.
        /// </summary>
        /// <param name="context">The current service context to use.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        internal NetworkServiceRestClient(ServiceClientContext context, IServiceLocator serviceLocator) : base(context, serviceLocator)
        {
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetNetworks()
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, NetworkVersionMoniker, NetworksUrlMoniker);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetFloatingIps()
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, NetworkVersionMoniker, FloatingIpsUrlMoniker);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetFloatingIp(string floatingIpId)
        {
            floatingIpId.AssertIsNotNullOrEmpty("floatingIpId", "Cannot get a floating ip with a null or empty id.");
            
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, NetworkVersionMoniker, FloatingIpsUrlMoniker, floatingIpId);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CreateFloatingIp(string networkId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, NetworkVersionMoniker, FloatingIpsUrlMoniker);
            client.Method = HttpMethod.Post;
            client.ContentType = "application/json";

            dynamic body = new ExpandoObject();
            dynamic networkIdProp = new ExpandoObject();
            networkIdProp.floating_network_id = networkId;
            body.floatingip = networkIdProp;
            string requestBody = JToken.FromObject(body).ToString();

            client.Content = requestBody.ConvertToStream();

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> DeleteFloatingIp(string floatingIpId)
        {
            floatingIpId.AssertIsNotNullOrEmpty("floatingIpId", "Cannot delete a floating ip with a null or empty id.");

            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, NetworkVersionMoniker, FloatingIpsUrlMoniker, floatingIpId);
            client.Method = HttpMethod.Delete;

            return await client.SendAsync();
        }
    }
}
