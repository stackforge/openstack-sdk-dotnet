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
using Openstack.Common;
using Openstack.Common.Http;
using Openstack.Common.ServiceLocation;
using Openstack.Identity;

namespace Openstack.Test.Identity
{
    [TestClass]
    public class IdentityServiceRestClientTests
    {
        internal IdentityRestServiceSimulator simulator;

        [TestInitialize]
        public void TestSetup()
        {
            //this is here to force the Object assembly to get loaded. This is a bug in the test runner.
            //var binder = typeof (Openstack.ServiceRegistrar);

            this.simulator = new IdentityRestServiceSimulator();

            ServiceLocator.Reset();
            var manager = ServiceLocator.Instance.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IHttpAbstractionClientFactory), new IdentityRestServiceSimulatorFactory(simulator));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.simulator = new IdentityRestServiceSimulator();
            ServiceLocator.Reset();
        }

        public IOpenstackCredential GetValidCredentials()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword".ConvertToSecureString();
            var tenantId = "12345";

            return new OpenstackCredential(endpoint, userName, password, tenantId);
        }

        [TestMethod]
        public async Task AuthenticationMethodAndUriAreValid()
        {
            var creds = GetValidCredentials();
            var client = new IdentityServiceRestClientFactory().Create(creds,CancellationToken.None);

            await client.Authenticate();

            Assert.AreEqual(creds.AuthenticationEndpoint, this.simulator.Uri);
            Assert.AreEqual(HttpMethod.Post, this.simulator.Method);
        }

        [TestMethod]
        public async Task AuthenticateIncludesCorrectHeaders()
        {
            var creds = GetValidCredentials();
            var client = new IdentityServiceRestClient(creds, CancellationToken.None);

            await client.Authenticate();

            Assert.IsTrue(this.simulator.Headers.ContainsKey("Accept"));
            Assert.AreEqual("application/json", this.simulator.Headers["Accept"]);
            Assert.AreEqual("application/json", this.simulator.ContentType);
        }

        [TestMethod]
        public async Task AuthenticateIncludesPayload()
        {
            var creds = GetValidCredentials();
            var client = new IdentityServiceRestClient(creds, CancellationToken.None);

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
            Assert.AreEqual(creds.Password.ConvertToUnsecureString(), password);
            Assert.AreEqual(creds.TenantId, tenantId);
        }
    
    }
}
