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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class IdentityServiceRestClientTests
    {
        internal IdentityRestServiceSimulator simulator;
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.simulator = new IdentityRestServiceSimulator();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IHttpAbstractionClientFactory), new IdentityRestServiceSimulatorFactory(simulator));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.simulator = new IdentityRestServiceSimulator();
            this.ServiceLocator = new ServiceLocator();
        }

        public IOpenStackCredential GetValidCredentials()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            return new OpenStackCredential(endpoint, userName, password, tenantId);
        }

        [TestMethod]
        public async Task AuthenticationMethodAndUriAreValid()
        {
            var creds = GetValidCredentials();
            var client = new IdentityServiceRestClientFactory().Create(creds,CancellationToken.None, this.ServiceLocator);

            await client.Authenticate();
            var expectedUri = new Uri(string.Format("{0}/tokens", creds.AuthenticationEndpoint));
            Assert.AreEqual(expectedUri, this.simulator.Uri);
            Assert.AreEqual(HttpMethod.Post, this.simulator.Method);
        }

        [TestMethod]
        public async Task AuthenticateIncludesCorrectHeaders()
        {
            var creds = GetValidCredentials();
            var client = new IdentityServiceRestClient(creds, CancellationToken.None, this.ServiceLocator);

            await client.Authenticate();

            Assert.IsTrue(this.simulator.Headers.ContainsKey("Accept"));
            Assert.AreEqual("application/json", this.simulator.Headers["Accept"]);
            Assert.AreEqual("application/json", this.simulator.ContentType);
        }

        [TestMethod]
        public async Task AuthenticateIncludesPayload()
        {
            var creds = GetValidCredentials();
            var client = new IdentityServiceRestClient(creds, CancellationToken.None, this.ServiceLocator);

            await client.Authenticate();

            Assert.IsNotNull(this.simulator.Content);

            var content = TestHelper.GetStringFromStream(this.simulator.Content);
            Assert.IsTrue(content.Length > 0);
        }

        [TestMethod]
        public void AuthenticationPayloadIsGeneratedCorrectly()
        {
            var creds = GetValidCredentials();
            var payload = IdentityServiceRestClient.CreateAuthenticationJsonPayload(creds);

            var obj = JObject.Parse(payload);
            var userName = obj["auth"]["passwordCredentials"]["username"];
            var password = obj["auth"]["passwordCredentials"]["password"];
            var tenantId = obj["auth"]["tenantName"];

            Assert.AreEqual(creds.UserName, userName);
            Assert.AreEqual(creds.Password, password);
            Assert.AreEqual(creds.TenantId, tenantId);
        }
    
    }
}
