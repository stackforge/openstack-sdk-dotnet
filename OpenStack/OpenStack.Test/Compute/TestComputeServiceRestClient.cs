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

using System.Collections.Generic;
using System.Threading.Tasks;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    public class TestComputeServiceRestClient : IComputeServiceRestClient
    {
        public TestComputeServiceRestClient()
        {
            this.Responses =  new Queue<IHttpResponseAbstraction>();
        }

        public Queue<IHttpResponseAbstraction> Responses { get; set; }

        public Task<IHttpResponseAbstraction> GetFlavors()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetFlavor(string flavorId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> AssignFloatingIp(string serverId, string ipAddress)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetServerMetadata(string serverId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> UpdateServerMetadata(string serverId, IDictionary<string, string> metadata)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> DeleteServerMetadata(string serverId, string key)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetKeyPairs()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetKeyPair(string keyPairName)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetImages()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetImage(string imageId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> DeleteImage(string imageId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetImageMetadata(string imageId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> UpdateImageMetadata(string imageId, IDictionary<string, string> metadata)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> DeleteImageMetadata(string imageId, string key)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        /// <inheritdoc/>
        public Task<IHttpResponseAbstraction> CreateServer(string name, string imageId, string flavorId, string networkId, IEnumerable<string> securityGroups)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> CreateServer(string name, string imageId, string flavorId, string networkId, string keyName,
            IEnumerable<string> securityGroups)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetServers()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetServer(string serverId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        /// <inheritdoc/>
        public Task<IHttpResponseAbstraction> DeleteServer(string serverId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }
    }

    public class TestComputeServiceRestClientFactory : IComputeServiceRestClientFactory
    {
        internal IComputeServiceRestClient Client;

        public TestComputeServiceRestClientFactory(IComputeServiceRestClient client)
        {
            this.Client = client;
        }

        public IComputeServiceRestClient Create(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            return Client;
        }
    }
}
