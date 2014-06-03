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

        public Task<IHttpResponseAbstraction> GetFlavor(string flavorId)
        {
            return Task.Factory.StartNew(() => Responses.Dequeue());
        }

        public Task<IHttpResponseAbstraction> GetFlavors()
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
