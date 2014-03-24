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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Openstack.Identity;

namespace Openstack.Test
{
    [TestClass]
    public class OpenstackClientManagerTests
    {
        internal class TestCredential : ICredential
        {
            public Uri AuthenticationEndpoint { get; private set; }
            public string AccessTokenId { get; private set; }

            public OpenstackServiceCatalog ServiceCatalog
            {
                get
                {
                    var catalog =
                        new OpenstackServiceCatalog
                        {
                            new OpenstackServiceDefinition("Test", "Test",
                                new List<OpenstackServiceEndpoint>()
                                {
                                    new OpenstackServiceEndpoint("http://someplace.com", "somewhere", "2.0.0.0",
                                        new Uri("http://someplace.com"), new Uri("http://someplace.com"))
                                })
                        };
                    return catalog;
                }
            }
        }
        
        internal class TestOpenstackClient : IOpenstackClient
        {
            #region Test Client Impl

            public IOpenstackCredential Credential { get; private set; }

            public Task Connect()
            {
                throw new NotImplementedException();
            }

            public void SetRegion(string region)
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>() where T : IOpenstackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>(string version) where T : IOpenstackServiceClient
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential, string version)
            {
                return credential is TestCredential;
            }

            #endregion
        }

        internal class OtherTestOpenstackClient : IOpenstackClient
        {
            #region Test Client Impl

            public IOpenstackCredential Credential { get; private set; }

            public Task Connect()
            {
                throw new NotImplementedException();
            }

            public void SetRegion(string region)
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>() where T : IOpenstackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>(string version) where T : IOpenstackServiceClient
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential, string version)
            {
                return credential is OpenstackCredential;
            }

            #endregion
        }

        internal class NonDefaultTestOpenstackClient : IOpenstackClient
        {
            public NonDefaultTestOpenstackClient(string parameter)
            {
                //forces a non-default ctor
            }

            #region Test Client Impl

            public IOpenstackCredential Credential { get; private set; }

            public Task Connect()
            {
                throw new NotImplementedException();
            }

            public void SetRegion(string region)
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>() where T : IOpenstackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>(string version) where T : IOpenstackServiceClient
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential, string version)
            {
                return credential is OpenstackCredential;
            }

            #endregion
        }

        [TestMethod]
        public void CanRegisterANewClient()
        {
            var manager = new OpenstackClientManager();
            manager.RegisterClient<TestOpenstackClient>();

            Assert.AreEqual(1, manager.clients.Count);
            Assert.AreEqual(typeof(TestOpenstackClient), manager.clients.First());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterTheSameClientTwice()
        {
            var manager = new OpenstackClientManager();
            manager.RegisterClient<TestOpenstackClient>();
            manager.RegisterClient<TestOpenstackClient>();
        }

        [TestMethod]
        public void CanRegisterMultipleClients()
        {
            var manager = new OpenstackClientManager();
            manager.RegisterClient<TestOpenstackClient>();
            manager.RegisterClient<OtherTestOpenstackClient>();

            Assert.AreEqual(2, manager.clients.Count);
            Assert.IsTrue(manager.clients.Contains(typeof(TestOpenstackClient)));
            Assert.IsTrue(manager.clients.Contains(typeof(OtherTestOpenstackClient)));
        }

        [TestMethod]
        public void CanListAvailableClients()
        {
            var manager = new OpenstackClientManager();
            manager.clients.Add(typeof(TestOpenstackClient));
            manager.clients.Add(typeof(OtherTestOpenstackClient));

            var clients = manager.ListAvailableClients().ToList();

            Assert.AreEqual(2, clients.Count());
            Assert.IsTrue(clients.Contains(typeof(TestOpenstackClient)));
            Assert.IsTrue(clients.Contains(typeof(OtherTestOpenstackClient)));
        }

        [TestMethod]
        public void CanCreateAClient()
        {
            var manager = new OpenstackClientManager();
            manager.clients.Add(typeof(TestOpenstackClient));

            var client = manager.CreateClient(new TestCredential());

            Assert.IsNotNull(client);
            Assert.IsInstanceOfType(client, typeof(TestOpenstackClient));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAClientIfCredentialIsNotSupported()
        {
            var manager = new OpenstackClientManager();
            manager.clients.Add(typeof(OtherTestOpenstackClient));

            manager.CreateClient(new TestCredential());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAClientIfNoClientsAreRegistered()
        {
            var manager = new OpenstackClientManager();
            manager.CreateClient(new TestCredential());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithNullCredential()
        {
            var manager = new OpenstackClientManager();
            manager.clients.Add(typeof(TestOpenstackClient));

            manager.CreateClient((ICredential)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithNullCredentialAndVersion()
        {
            var manager = new OpenstackClientManager();
            manager.clients.Add(typeof(TestOpenstackClient));

            manager.CreateClient(null, "1.0.0..0");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithCredentialAndNullVersion()
        {
            var manager = new OpenstackClientManager();
            manager.clients.Add(typeof(TestOpenstackClient));

            manager.CreateClient(new TestCredential(), null);
        }

        [TestMethod]
        public void CanCreateAnInstanceOfAClient()
        {
            var manager = new OpenstackClientManager();

            var client = manager.CreateClient(typeof (TestOpenstackClient));
            Assert.IsNotNull(client);
            Assert.IsInstanceOfType(client, typeof(TestOpenstackClient));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanCreateAnInstanceOfAClientWithNullType()
        {
            var manager = new OpenstackClientManager();

            manager.CreateClient((Type)null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAnInstanceOfANonOpenstackClient()
        {
            var manager = new OpenstackClientManager();
            manager.CreateClient(typeof(Object));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAnInstanceOfAOpenstackClientWithoutADefaultCtor()
        {
            var manager = new OpenstackClientManager();

            manager.CreateClient(typeof(NonDefaultTestOpenstackClient));
        }
    }
}
