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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class IdentityServiceClientTests
    {
        internal TestIdentityServicePocoClient pocoClient;
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.ServiceLocator = new ServiceLocator();
            this.pocoClient = new TestIdentityServicePocoClient();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IIdentityServicePocoClientFactory), new TestIdentityServicePocoClientFactory(pocoClient));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.pocoClient = new TestIdentityServicePocoClient();
            this.ServiceLocator = new ServiceLocator();
        }

        public IOpenStackCredential GetValidCredentials()
        {
            var endpoint = new Uri("https://someidentityendpoint:35357/v2.0/tokens");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            return new OpenStackCredential(endpoint, userName, password, tenantId);
        }

        [TestMethod]
        public async Task CanAuthenticate()
        {
            var creds = GetValidCredentials();
            var token = "someToken";

            this.pocoClient.AuthenticationDelegate = () =>
            {
                creds.SetAccessTokenId(token);
                return Task.Factory.StartNew(() => creds);
            };

            var client = new IdentityServiceClientDefinition().Create(GetValidCredentials(), string.Empty, CancellationToken.None, this.ServiceLocator) as IdentityServiceClient;
            var resp = await client.Authenticate();

            Assert.AreEqual(creds, resp);
            Assert.AreEqual(creds,client.Credential);
        }
    }
}
