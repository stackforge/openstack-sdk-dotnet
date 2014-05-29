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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test
{
    [TestClass]
    public class OpenStackClientManagerTests
    {
        internal class TestCredential : ICredential
        {
            public Uri AuthenticationEndpoint { get; private set; }
            public string AccessTokenId { get; private set; }

            public OpenStackServiceCatalog ServiceCatalog
            {
                get
                {
                    var catalog =
                        new OpenStackServiceCatalog
                        {
                            new OpenStackServiceDefinition("Test", "Test",
                                new List<OpenStackServiceEndpoint>()
                                {
                                    new OpenStackServiceEndpoint("http://someplace.com", "somewhere", "2.0.0.0",
                                        "http://www.someplace.com", "http://www.someplace.com")
                                })
                        };
                    return catalog;
                }
            }
        }
        
        internal class TestOpenStackClient : IOpenStackClient
        {
            #region Test Client Impl

            public IOpenStackCredential Credential { get; private set; }

            public TestOpenStackClient(ICredential cred, CancellationToken token, IServiceLocator locator)
            {
                
            }

            public Task Connect()
            {
                throw new NotImplementedException();
            }

            public void SetRegion(string region)
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>() where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>(string version) where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClientByName<T>(string serviceName) where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClientByName<T>(string serviceName, string version) where T : IOpenStackServiceClient
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

        internal class OtherTestOpenStackClient : IOpenStackClient
        {
            #region Test Client Impl

            public IOpenStackCredential Credential { get; private set; }

            public Task Connect()
            {
                throw new NotImplementedException();
            }

            public void SetRegion(string region)
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>() where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>(string version) where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClientByName<T>(string serviceName) where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClientByName<T>(string serviceName, string version) where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential, string version)
            {
                return credential is OpenStackCredential;
            }

            #endregion
        }

        internal class NonDefaultTestOpenStackClient : IOpenStackClient
        {
            public NonDefaultTestOpenStackClient(string parameter)
            {
                //forces a non-default ctor
            }

            #region Test Client Impl

            public IOpenStackCredential Credential { get; private set; }

            public Task Connect()
            {
                throw new NotImplementedException();
            }

            public void SetRegion(string region)
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>() where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClient<T>(string version) where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClientByName<T>(string serviceName) where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public T CreateServiceClientByName<T>(string serviceName, string version) where T : IOpenStackServiceClient
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential, string version)
            {
                return credential is OpenStackCredential;
            }

            #endregion
        }

        [TestMethod]
        public void CanRegisterANewClient()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.RegisterClient<TestOpenStackClient>();

            Assert.AreEqual(1, manager.clients.Count);
            Assert.AreEqual(typeof(TestOpenStackClient), manager.clients.First());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterTheSameClientTwice()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.RegisterClient<TestOpenStackClient>();
            manager.RegisterClient<TestOpenStackClient>();
        }

        [TestMethod]
        public void CanRegisterMultipleClients()
        {
            //var manager = OpenStackClientManager.Instance as OpenStackClientManager;
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.RegisterClient<TestOpenStackClient>();
            manager.RegisterClient<OtherTestOpenStackClient>();

            Assert.AreEqual(2, manager.clients.Count);
            Assert.IsTrue(manager.clients.Contains(typeof(TestOpenStackClient)));
            Assert.IsTrue(manager.clients.Contains(typeof(OtherTestOpenStackClient)));
        }

        [TestMethod]
        public void CanListAvailableClients()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.clients.Add(typeof(TestOpenStackClient));
            manager.clients.Add(typeof(OtherTestOpenStackClient));

            var clients = manager.ListAvailableClients().ToList();

            Assert.AreEqual(2, clients.Count());
            Assert.IsTrue(clients.Contains(typeof(TestOpenStackClient)));
            Assert.IsTrue(clients.Contains(typeof(OtherTestOpenStackClient)));
        }

        [TestMethod]
        public void CanCreateAClient()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.clients.Add(typeof(TestOpenStackClient));

            var client = manager.CreateClient(new TestCredential());

            Assert.IsNotNull(client);
            Assert.IsInstanceOfType(client, typeof(TestOpenStackClient));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAClientIfCredentialIsNotSupported()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.clients.Add(typeof(OtherTestOpenStackClient));

            manager.CreateClient(new TestCredential());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAClientIfNoClientsAreRegistered()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.CreateClient(new TestCredential());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithNullCredential()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.clients.Add(typeof(TestOpenStackClient));

            manager.CreateClient((ICredential)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithNullCredentialAndVersion()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.clients.Add(typeof(TestOpenStackClient));

            manager.CreateClient(null, "1.0.0..0");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAClientWithCredentialAndNullVersion()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            manager.clients.Add(typeof(TestOpenStackClient));

            manager.CreateClient(new TestCredential(), null);
        }

        [TestMethod]
        public void CanCreateAnInstanceOfAClient()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            var creds = new OpenStackCredential(new Uri("http://someurl.com"), "user", "password", "12345");

            var client = manager.CreateClientInstance(typeof(TestOpenStackClient), creds, CancellationToken.None);
            Assert.IsNotNull(client);
            Assert.IsInstanceOfType(client, typeof(TestOpenStackClient));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanCreateAnInstanceOfAClientWithNullType()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            var creds = new OpenStackCredential(new Uri("http://someurl.com"), "user", "password", "12345");

            manager.CreateClientInstance((Type)null, creds, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAnInstanceOfANonOpenStackClient()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            var creds = new OpenStackCredential(new Uri("http://someurl.com"), "user", "password", "12345");
            manager.CreateClientInstance(typeof(Object), creds, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAnInstanceOfAOpenStackClientWithoutADefaultCtor()
        {
            var manager = new OpenStackClientManager(new ServiceLocator());
            var creds = new OpenStackCredential(new Uri("http://someurl.com"), "user", "password", "12345");

            manager.CreateClientInstance(typeof(NonDefaultTestOpenStackClient), creds, CancellationToken.None);
        }
    }
}
