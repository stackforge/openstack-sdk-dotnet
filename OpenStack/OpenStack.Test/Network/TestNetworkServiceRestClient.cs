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
using OpenStack.Network;

namespace OpenStack.Test.Network
{
    public class TestNetworkServiceRestClient : INetworkServiceRestClient
    {
        public TestNetworkServiceRestClient()
        {
            this.Responses =  new Queue<IHttpResponseAbstraction>();
        }

        public Queue<IHttpResponseAbstraction> Responses { get; set; }

        public Task<IHttpResponseAbstraction> GetNetworks()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetFloatingIps()
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetFloatingIp(string floatingIpId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> CreateFloatingIp(string networkId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> DeleteFloatingIp(string floatingIpId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }
    }

    public class TestNetworkServiceRestClientFactory : INetworkServiceRestClientFactory
    {
        internal INetworkServiceRestClient Client;

        public TestNetworkServiceRestClientFactory(INetworkServiceRestClient client)
        {
            this.Client = client;
        }

        public INetworkServiceRestClient Create(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            return Client;
        }
    }
}
