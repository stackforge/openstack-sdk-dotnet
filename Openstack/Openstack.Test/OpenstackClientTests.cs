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
using Openstack.Common.ServiceLocation;
using Openstack.Identity;

namespace Openstack.Test
{
    [TestClass]
    public class OpenstackClientTests
    {
        internal class TestIdentityServiceClient : IIdentityServiceClient
        {
            internal IOpenstackCredential cred;
            internal CancellationToken token;

            public TestIdentityServiceClient(IOpenstackCredential cred, CancellationToken token)
            {
                this.cred = cred;
                this.token = token;
            }

            public async Task<IOpenstackCredential> Authenticate()
            {
                return await Task.Run(() =>
                {
                    this.cred.SetAccessTokenId("12345");
                    return cred;
                } );
            }
        }

        internal class TestIdentityServiceClientDefinition : IOpenstackServiceClientDefinition
        {
            public string Name { get; private set; }
            
            public IOpenstackServiceClient Create(ICredential credential, CancellationToken cancellationToken)
            {
                return new TestIdentityServiceClient((IOpenstackCredential)credential, cancellationToken);
            }

            public IEnumerable<string> ListSupportedVersions()
            {
                return new List<string>();
            }

            public bool IsSupported(ICredential credential)
            {
                return true;
            }
        }

        [TestInitialize]
        public void TestSetup()
        {
           ServiceLocator.Reset();

            var manager = ServiceLocator.Instance.Locate<IServiceLocationOverrideManager>();

            var serviceManager = new OpenstackServiceClientManager();
            serviceManager.RegisterServiceClient<TestIdentityServiceClient>(new TestIdentityServiceClientDefinition());
            manager.RegisterServiceInstance(typeof(IOpenstackServiceClientManager), serviceManager);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ServiceLocator.Reset();
        }

        [TestMethod]
        public async Task CanConnect()
        {
            var client =
                new OpenstackClient(
                    new OpenstackCredential(new Uri("http://someplace.org"), "someuser", new SecureString(),
                        "sometenant"), CancellationToken.None);
            await client.Connect();
            Assert.AreEqual("12345", client.Credential.AccessTokenId);
        }

        [TestMethod]
        public void CanSetRegion()
        {
            var expectedRegion = "newregion";
            var client = new OpenstackClient( 
                    new OpenstackCredential(new Uri("http://someplace.org"), "someuser", new SecureString(),
                        "sometenant","oldregion"), CancellationToken.None);
            client.SetRegion(expectedRegion);

            Assert.AreEqual(expectedRegion, client.Credential.Region);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotSetRegionWithNull()
        {
            var client = new OpenstackClient(
                    new OpenstackCredential(new Uri("http://someplace.org"), "someuser", new SecureString(),
                        "sometenant", "oldregion"), CancellationToken.None);
            client.SetRegion(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotSetRegionWithEmptyString()
        {
            var client = new OpenstackClient(
                    new OpenstackCredential(new Uri("http://someplace.org"), "someuser", new SecureString(),
                        "sometenant", "oldregion"), CancellationToken.None);
            client.SetRegion(string.Empty);
        }

        [TestMethod]
        public void CanSupportAnyVersion()
        {
            var client = new OpenstackClient();
            var versions = client.GetSupportedVersions();
            Assert.IsTrue(versions.Contains("Any"));
        }

        [TestMethod]
        public void CanSupportOpenstackCredential()
        {
            var cred = new OpenstackCredential(new Uri("http://someplace.org"), "someuser", new SecureString(), "sometenant");
            var client = new OpenstackClient();
            Assert.IsTrue(client.IsSupported(cred,string.Empty));
        }

        [TestMethod]
        public void CannotSupportNullCredential()
        {
            var client = new OpenstackClient();
            Assert.IsFalse(client.IsSupported(null, string.Empty));
        }

        [TestMethod]
        public void CanSupportNullVersion()
        {
            var cred = new OpenstackCredential(new Uri("http://someplace.org"), "someuser", new SecureString(), "sometenant");
            var client = new OpenstackClient();
            Assert.IsTrue(client.IsSupported(cred, null));
        }
    }
}
