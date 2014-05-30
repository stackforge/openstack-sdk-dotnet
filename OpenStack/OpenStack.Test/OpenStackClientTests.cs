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
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test
{
    [TestClass]
    public class OpenStackClientTests
    {
        internal IServiceLocator ServiceLocator;

        internal class TestIdentityServiceClient : IIdentityServiceClient
        {
            internal IOpenStackCredential cred;
            internal CancellationToken token;

            public TestIdentityServiceClient(IOpenStackCredential cred, CancellationToken token)
            {
                this.cred = cred;
                this.token = token;
            }

            public async Task<IOpenStackCredential> Authenticate()
            {
                return await Task.Run(() =>
                {
                    this.cred.SetAccessTokenId("12345");
                    return cred;
                } );
            }
        }

        internal class TestIdentityServiceClientDefinition : IOpenStackServiceClientDefinition
        {
            public string Name { get; private set; }
            
            public IOpenStackServiceClient Create(ICredential credential, string serviceName, CancellationToken cancellationToken, IServiceLocator serviceLocator)
            {
                return new TestIdentityServiceClient((IOpenStackCredential)credential, cancellationToken);
            }

            public IEnumerable<string> ListSupportedVersions()
            {
                return new List<string>();
            }

            public bool IsSupported(ICredential credential, string serviceName)
            {
                return true;
            }
        }

        [TestInitialize]
        public void TestSetup()
        {
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            var serviceManager = new OpenStackServiceClientManager(this.ServiceLocator);
            serviceManager.RegisterServiceClient<TestIdentityServiceClient>(new TestIdentityServiceClientDefinition());
            manager.RegisterServiceInstance(typeof(IOpenStackServiceClientManager), serviceManager);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.ServiceLocator = new ServiceLocator();
        }

        [TestMethod]
        public async Task CanConnect()
        {
            var client =
                new OpenStackClient(
                    new OpenStackCredential(new Uri("http://someplace.org"), "someuser", "password",
                        "sometenant"), CancellationToken.None, this.ServiceLocator);
            await client.Connect();
            Assert.AreEqual("12345", client.Credential.AccessTokenId);
        }

        [TestMethod]
        public void CanSetRegion()
        {
            var expectedRegion = "newregion";
            var client = new OpenStackClient(
                    new OpenStackCredential(new Uri("http://someplace.org"), "someuser", "password",
                        "sometenant", "oldregion"), CancellationToken.None, this.ServiceLocator);
            client.SetRegion(expectedRegion);

            Assert.AreEqual(expectedRegion, client.Credential.Region);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotSetRegionWithNull()
        {
            var client = new OpenStackClient(
                    new OpenStackCredential(new Uri("http://someplace.org"), "someuser", "password",
                        "sometenant", "oldregion"), CancellationToken.None, this.ServiceLocator);
            client.SetRegion(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotSetRegionWithEmptyString()
        {
            var client = new OpenStackClient(
                    new OpenStackCredential(new Uri("http://someplace.org"), "someuser", "password",
                        "sometenant", "oldregion"), CancellationToken.None, this.ServiceLocator);
            client.SetRegion(string.Empty);
        }

        [TestMethod]
        public void CanSupportAnyVersion()
        {
            var client = new OpenStackClient();
            var versions = client.GetSupportedVersions();
            Assert.IsTrue(versions.Contains("Any"));
        }

        [TestMethod]
        public void CanSupportOpenStackCredential()
        {
            var cred = new OpenStackCredential(new Uri("http://someplace.org"), "someuser", "password", "sometenant");
            var client = new OpenStackClient();
            Assert.IsTrue(client.IsSupported(cred,string.Empty));
        }

        [TestMethod]
        public void CannotSupportNullCredential()
        {
            var client = new OpenStackClient();
            Assert.IsFalse(client.IsSupported(null, string.Empty));
        }

        [TestMethod]
        public void CanSupportNullVersion()
        {
            var cred = new OpenStackCredential(new Uri("http://someplace.org"), "someuser", "password", "sometenant");
            var client = new OpenStackClient();
            Assert.IsTrue(client.IsSupported(cred, null));
        }
    }
}
