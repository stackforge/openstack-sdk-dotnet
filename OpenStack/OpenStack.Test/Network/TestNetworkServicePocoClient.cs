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
using System.Threading.Tasks;
using OpenStack.Common.ServiceLocation;
using OpenStack.Network;

namespace OpenStack.Test.Network
{
    public class TestNetworkServicePocoClient : INetworkServicePocoClient
    {
        public Func<Task<IEnumerable<OpenStack.Network.Network>>> GetNetworksDelegate { get; set; }

        public Func<Task<IEnumerable<OpenStack.Network.FloatingIp>>> GetFloatingIpsDelegate { get; set; }

        public Func<string, Task<OpenStack.Network.FloatingIp>> GetFloatingIpDelegate { get; set; }

        public Func<string, Task<OpenStack.Network.FloatingIp>> CreateFloatingIpDelegate { get; set; }

        public Func<string, Task> DeleteFloatingIpDelegate { get; set; }

        public async Task<IEnumerable<OpenStack.Network.Network>> GetNetworks()
        {
            return await this.GetNetworksDelegate();
        }

        public async Task<IEnumerable<FloatingIp>> GetFloatingIps()
        {
            return await GetFloatingIpsDelegate();
        }

        public async Task<FloatingIp> GetFloatingIp(string floatingIpId)
        {
            return await GetFloatingIpDelegate(floatingIpId);
        }

        public async Task<FloatingIp> CreateFloatingIp(string networkId)
        {
            return await CreateFloatingIpDelegate(networkId);
        }

        public async Task DeleteFloatingIp(string floatingIpId)
        {
            await DeleteFloatingIpDelegate(floatingIpId);
        }
    }

    public class TestNetworkServicePocoClientFactory : INetworkServicePocoClientFactory
    {
        internal INetworkServicePocoClient client;

        public TestNetworkServicePocoClientFactory(INetworkServicePocoClient client)
        {
            this.client = client;
        }

        public INetworkServicePocoClient Create(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            return client;
        }
    }
}
