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

using System.Threading;
using System.Threading.Tasks;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    public class TestIdentityServiceRestClient : IIdentityServiceRestClient
    {
        public IHttpResponseAbstraction Response { get; set; }

        public Task<IHttpResponseAbstraction> Authenticate()
        {
            return Task.Factory.StartNew(() => Response);
        }
    }

    public class TestIdentityServiceRestClientFactory : IIdentityServiceRestClientFactory
    {
        internal IIdentityServiceRestClient Client;

        public TestIdentityServiceRestClientFactory(IIdentityServiceRestClient client)
        {
            this.Client = client;
        }

        public IIdentityServiceRestClient Create(IOpenStackCredential credential, CancellationToken cancellationToken, IServiceLocator serviceLocator)
        {
            return Client;
        }
    }
}
