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
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    public class TestIdentityServicePocoClient : IIdentityServicePocoClient
    {
        public Func<Task<IOpenStackCredential>> AuthenticationDelegate { get; set; }

        public Task<IOpenStackCredential> Authenticate()
        {
            return AuthenticationDelegate();
        }
    }

    public class TestIdentityServicePocoClientFactory : IIdentityServicePocoClientFactory
    {
        internal IIdentityServicePocoClient client;

        public TestIdentityServicePocoClientFactory(IIdentityServicePocoClient client)
        {
            this.client = client;
        }

        public IIdentityServicePocoClient Create(IOpenStackCredential credentials, string serviceName, CancellationToken token, IServiceLocator serviceLocator)
        {
            return client;
        }
    }
}
