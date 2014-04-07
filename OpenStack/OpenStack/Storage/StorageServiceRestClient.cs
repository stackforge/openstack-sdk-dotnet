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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Storage
{
    /// <inheritdoc/>
    internal class StorageServiceRestClient : IStorageServiceRestClient
    {
        internal StorageServiceClientContext context;
        internal IStorageContainerNameValidator StorageContainerNameValidator;

        /// <summary>
        /// Creates a new instance of the StorageServiceRestClient class.
        /// </summary>
        /// <param name="context"></param>
        internal StorageServiceRestClient(StorageServiceClientContext context)
        {
            this.StorageContainerNameValidator = ServiceLocator.Instance.Locate<IStorageContainerNameValidator>();
            this.context = context;
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CreateObject(string containerName, string objectName, IDictionary<string,string> metadata, Stream content)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName, objectName);
            client.Method = HttpMethod.Put;

            this.AddItemMetadata(metadata, client);

            client.Content = content;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CreateDynamicManifest(string containerName, string manifestName, IDictionary<string, string> metadata, string segmentsPath)
        {
            AssertContainerNameIsValid(containerName);
            manifestName.AssertIsNotNullOrEmpty("manifestName","Cannot create a storage manifest with a null or empty name.");
            segmentsPath.AssertIsNotNullOrEmpty("segmentsPath","Cannot create a dynamic large object manifest with a null or empty segments path.");
            metadata.AssertIsNotNull("metadata","Cannot create a storage manifest with null metadata.");

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName, manifestName);
            client.Method = HttpMethod.Put;
            client.Content = new MemoryStream();

            client.Headers.Add("X-Object-Manifest", segmentsPath);
            this.AddItemMetadata(metadata, client);

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CreateStaticManifest(string containerName, string manifestName, IDictionary<string, string> metadata, Stream content)
        {
            AssertContainerNameIsValid(containerName);
            manifestName.AssertIsNotNullOrEmpty("manifestName", "Cannot create a storage manifest with a null or empty name.");
            metadata.AssertIsNotNull("metadata", "Cannot create a storage manifest with null metadata.");
            content.AssertIsNotNull("content","Cannot create a static large object manifest with null content.");

            var client = this.GetHttpClient(this.context);

            var baseUri = CreateRequestUri(GetServiceEndpoint(this.context), containerName);
            client.Uri = new Uri(string.Format("{0}/{1}?multipart-manifest=put", baseUri, manifestName));
            client.Method = HttpMethod.Put;
            client.Content = content;

            this.AddItemMetadata(metadata, client);

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CreateContainer(string containerName, IDictionary<string, string> metadata)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName);
            client.Method = HttpMethod.Put;
            client.Content = new MemoryStream();

            this.AddItemMetadata(metadata, client);

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetObject(string containerName, string objectName)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName, objectName);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetFolder(string containerName, string folderName)
        {
            AssertContainerNameIsValid(containerName);
            folderName.AssertIsNotNullOrEmpty("folderName","Cannot get a folder with a null or empty folder name.");

            var client = this.GetHttpClient(this.context);

            var baseUri = CreateRequestUri(GetServiceEndpoint(this.context), containerName);
            var prefix = string.Compare("/", folderName, StringComparison.InvariantCulture) == 0
                ? string.Empty
                : string.Format("&prefix={0}", folderName);
            
            client.Uri = new Uri(string.Format("{0}?delimiter=/{1}", baseUri, prefix));
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetContainer(string containerName)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName);
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> DeleteObject(string containerName, string objectName)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName, objectName);
            client.Method = HttpMethod.Delete;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> DeleteContainer(string containerName)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName);
            client.Method = HttpMethod.Delete;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> UpdateObject(string containerName, string objectName, IDictionary<string, string> metadata)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName, objectName);
            client.Method = HttpMethod.Post;
            AddItemMetadata(metadata,client);

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> UpdateContainer(string containerName, IDictionary<string, string> metadata)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName);
            client.Method = HttpMethod.Post;
            AddItemMetadata(metadata, client);

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetContainerMetadata(string containerName)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName);
            client.Method = HttpMethod.Head;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetObjectMetadata(string containerName, string objectName)
        {
            AssertContainerNameIsValid(containerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), containerName, objectName);
            client.Method = HttpMethod.Head;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetManifestMetadata(string containerName, string manifestName)
        {
            AssertContainerNameIsValid(containerName);
            manifestName.AssertIsNotNullOrEmpty("manifestName", "Cannot get a manifest with a null or empty folder name.");

            var client = this.GetHttpClient(this.context);

            var baseUri = CreateRequestUri(GetServiceEndpoint(this.context), containerName);
            client.Uri = new Uri(string.Format("{0}/{1}?multipart-manifest=get", baseUri, manifestName));
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> CopyObject(string sourceContainerName, string sourceObjectName, string targetContainerName, string targetObjectName)
        {
            AssertContainerNameIsValid(sourceContainerName);
            AssertContainerNameIsValid(targetContainerName);

            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context), sourceContainerName, sourceObjectName);
            client.Headers.Add("Destination",string.Join("/", targetContainerName, targetObjectName));

            client.Method = new HttpMethod("COPY");

            return await client.SendAsync();
        }

        /// <inheritdoc/>
        public async Task<IHttpResponseAbstraction> GetAccount()
        {
            var client = this.GetHttpClient(this.context);

            client.Uri = CreateRequestUri(GetServiceEndpoint(this.context));
            client.Method = HttpMethod.Get;

            return await client.SendAsync();
        }

        /// <summary>
        /// Creates an Http client to communicate with the remote OpenStack service.
        /// </summary>
        /// <param name="context">The storage context to use when creating the client.</param>
        /// <returns>The Http client.</returns>
        internal IHttpAbstractionClient GetHttpClient(StorageServiceClientContext context)
        {
            var client = ServiceLocator.Instance.Locate<IHttpAbstractionClientFactory>().Create(context.CancellationToken);
            AddAuthenticationHeader(context.Credential.AccessTokenId, client);
            client.Headers.Add("Accept","application/json");
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
            var temp = new List<string> {endpoint.AbsoluteUri};
            temp.AddRange(values);
            return new Uri(string.Join("/", temp.ToArray()));
        }

        /// <summary>
        /// Adds the appropriate heads to the Http client for the given items metadata.
        /// </summary>
        /// <param name="metadata">The items metadata.</param>
        /// <param name="client">The http client.</param>
        internal void AddItemMetadata( IDictionary<string, string> metadata, IHttpAbstractionClient client)
        {
             metadata.Keys.ToList().ForEach((k) => client.Headers.Add(string.Format("X-Object-Meta-{0}", k), metadata[k]));
        }

        /// <summary>
        /// Asserts that the given container name is valid.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        internal void AssertContainerNameIsValid(string containerName)
        {
            if (!StorageContainerNameValidator.Validate(containerName))
            {
                throw new ArgumentException(string.Format("Container name '{0}' is invalid. Container names cannot includes slashes.", containerName), "containerName");
            }
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

        /// <summary>
        /// Gets the public endpoint for the remote service.
        /// </summary>
        /// <param name="context">The storage service context to use.</param>
        /// <returns>The public endpoint for the remote storage service.</returns>
        internal static Uri GetServiceEndpoint(StorageServiceClientContext context)
        {
            return context.Credential.ServiceCatalog.GetPublicEndpoint(context.StorageServiceName, context.Credential.Region);
        }
    }
}
