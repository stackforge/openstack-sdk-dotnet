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

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
        internal const string MetadataUrlMoniker = "metadata";
        internal const string ImagesUrlMoniker = "images";
        internal const string FlavorsUrlMoniker = "flavors";
        internal const string ServersUrlMoniker = "servers";
        internal const string ActionUrlMoniker = "action";
        internal const string KeyPairUrlMoniker = "os-keypairs";

        /// <summary>
        /// Creates a new instance of the ComputeServiceRestClient class.
        /// </summary>
        /// <param name="context">The current service client context to use.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        internal ComputeServiceRestClient(ServiceClientContext context, IServiceLocator serviceLocator)
            : base(context, serviceLocator)
        {
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetFlavors()
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, FlavorsUrlMoniker);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetFlavor(string flavorId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, FlavorsUrlMoniker, flavorId);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetServers()
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, ServersUrlMoniker);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetServer(string serverId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, ServersUrlMoniker, serverId);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> DeleteServer(string serverId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, ServersUrlMoniker, serverId);
            client.Method = HttpMethod.Delete;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> AssignFloatingIp(string serverId, string ipAddress)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, ServersUrlMoniker, serverId, ActionUrlMoniker);
            client.Method = HttpMethod.Post;

            var requestBody = this.GenerateAssignFloatingIpRequestBody(ipAddress);
            client.Content = requestBody.ConvertToStream();
            client.ContentType = "application/json";

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetServerMetadata(string serverId)
        {
            return await GetItemMetadata(ServersUrlMoniker, serverId);
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> UpdateServerMetadata(string serverId,
            IDictionary<string, string> metadata)
        {
            return await UpdateItemMetadata(ServersUrlMoniker, serverId, metadata);
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> DeleteServerMetadata(string serverId, string key)
        {
            return await DeleteItemMetadata(ServersUrlMoniker, serverId, key);
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetKeyPairs()
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, KeyPairUrlMoniker);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetKeyPair(string keyPairName)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, KeyPairUrlMoniker, keyPairName);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetImages()
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, ImagesUrlMoniker, "detail");
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetImage(string imageId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, ImagesUrlMoniker, imageId);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> DeleteImage(string imageId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, ImagesUrlMoniker, imageId);
            client.Method = HttpMethod.Delete;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetImageMetadata(string imageId)
        {
            return await GetItemMetadata(ImagesUrlMoniker, imageId);
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> UpdateImageMetadata(string imageId,
            IDictionary<string, string> metadata)
        {
            return await UpdateItemMetadata(ImagesUrlMoniker, imageId, metadata);
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> DeleteImageMetadata(string imageId, string key)
        {
            return await DeleteItemMetadata(ImagesUrlMoniker, imageId, key);
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CreateServer(string name, string imageId, string flavorId, string networkId, string keyName,
            IEnumerable<string> securityGroups)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, ServersUrlMoniker);
            client.Method = HttpMethod.Post;

            var requestBody = this.GenerateCreateServerRequestBody(name, imageId, flavorId, networkId, keyName, securityGroups);
            client.Content = requestBody.ConvertToStream();
            client.ContentType = "application/json";

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        internal async Task<IHttpResponseAbstraction> DeleteItemMetadata(string itemType, string itemId, string key)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, itemType, itemId, MetadataUrlMoniker, key);
            client.Method = HttpMethod.Delete;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        internal async Task<IHttpResponseAbstraction> UpdateItemMetadata(string itemType, string itemId, IDictionary<string, string> metadata)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, itemType, itemId, MetadataUrlMoniker);
            client.ContentType = "application/json";
            client.Method = HttpMethod.Post;

            var converter = this.ServiceLocator.Locate<IComputeItemMetadataPayloadConverter>();
            var payload = converter.Convert(metadata);

            client.Content = payload.ConvertToStream();

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        internal async Task<IHttpResponseAbstraction> GetItemMetadata(string itemType, string itemId)
        {
            var client = this.GetHttpClient(this.Context);

            client.Uri = CreateRequestUri(this.Context.PublicEndpoint, itemType, itemId, MetadataUrlMoniker);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <summary>
        /// Generates a request body for creating compute servers.
        /// </summary>
        /// <param name="name">The name of the server.</param>
        /// <param name="imageId">The id of the image to use.</param>
        /// <param name="flavorId">The id of the flavor to use.</param>
        /// <param name="networkId">The id of the network to attach the server to.</param>
        /// <param name="keyName">The name of the key to associate this server with.</param>
        /// <param name="securityGroups">A list of security groups to associate the server with.</param>
        /// <returns>A json encoded request body.</returns>
        internal string GenerateCreateServerRequestBody(string name, string imageId, string flavorId, string networkId, string keyName, IEnumerable<string> securityGroups)
        {
            var secGroups = new List<dynamic>();
            foreach (var g in securityGroups)
            {
                dynamic group = new System.Dynamic.ExpandoObject();
                group.name = g;
                secGroups.Add(group);
            }

            dynamic network = new System.Dynamic.ExpandoObject();
            network.uuid = networkId;

            dynamic server = new System.Dynamic.ExpandoObject();
            server.name = name;
            server.imageRef = imageId;
            server.flavorRef = flavorId;
            server.max_count = "1";
            server.min_count = "1";
            
            if (!string.IsNullOrEmpty(keyName))
            {
                server.key_name = keyName;
            }

            server.networks = new List<dynamic>() { network };
            server.security_groups = secGroups;

            dynamic body = new System.Dynamic.ExpandoObject();
            body.server = server;
            return JToken.FromObject(body).ToString();
        }

        /// <summary>
        /// Generates a request body for creating compute servers.
        /// </summary>
        /// <param name="ipAddress">The ip address to assign.</param>
        /// <returns>A json encoded request body.</returns>
        internal string GenerateAssignFloatingIpRequestBody(string ipAddress)
        {
            dynamic addIpAddress = new System.Dynamic.ExpandoObject();
            addIpAddress.address = ipAddress;

            dynamic body = new System.Dynamic.ExpandoObject();
            body.addFloatingIp = addIpAddress;
            return JToken.FromObject(body).ToString();
        }
    }
}
