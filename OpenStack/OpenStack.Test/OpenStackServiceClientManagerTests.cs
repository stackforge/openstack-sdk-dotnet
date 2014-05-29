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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test
{
    [TestClass]
    public class OpenStackServiceClientManagerTests
    {
        internal class TestOpenStackServiceClient : IOpenStackServiceClient
        {
            public TestOpenStackServiceClient(ICredential credential, CancellationToken token)
            {
                
            }
        }

        internal class TestOpenStackServiceClientDefinition : IOpenStackServiceClientDefinition
        {
            public string Name { get; private set; }
            
            public IOpenStackServiceClient Create(ICredential credential, string serviceName, CancellationToken token, IServiceLocator serviceLocator)
            {
                return new TestOpenStackServiceClient(credential, token);
            }

            public IEnumerable<string> ListSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential, string serviceName)
            {
                return true;
            }
        }

        internal class OtherTestOpenStackServiceClient : IOpenStackServiceClient
        {
            public OtherTestOpenStackServiceClient(ICredential credential, CancellationToken token)
            {
                
            }

            public IEnumerable<string> ListSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported()
            {
                return false;
            }

            public static IOpenStackServiceClient Create(ICredential credential, CancellationToken token)
            {
                return new OtherTestOpenStackServiceClient(credential, token);
            }
        }

        internal class OtherTestOpenStackServiceClientDefinition : IOpenStackServiceClientDefinition
        {
            public string Name { get; private set; }

            public IOpenStackServiceClient Create(ICredential credential, string serviceName, CancellationToken token, IServiceLocator serviceLocator)
            {
                return new OtherTestOpenStackServiceClient(credential, token);
            }

            public IEnumerable<string> ListSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential, string serviceName)
            {
                return false;
            }
        }

        internal class NoValidCtroTestOpenStackServiceClient : IOpenStackServiceClient
        {
            public IEnumerable<string> ListSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported()
            {
                return true;
            }
        }

        [TestMethod]
        public void CanRegisterANewService()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.RegisterServiceClient<TestOpenStackServiceClient>(new TestOpenStackServiceClientDefinition());

            Assert.AreEqual(1, manager.serviceClientDefinitions.Count);
            Assert.IsTrue(manager.serviceClientDefinitions.ContainsKey(typeof(TestOpenStackServiceClient)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterTheSameServiceTwice()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.RegisterServiceClient<TestOpenStackServiceClient>(new TestOpenStackServiceClientDefinition());
            manager.RegisterServiceClient<TestOpenStackServiceClient>(new TestOpenStackServiceClientDefinition());
        }

        [TestMethod]
        public void CanRegisterMultipleServices()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.RegisterServiceClient<TestOpenStackServiceClient>(new TestOpenStackServiceClientDefinition());
            manager.RegisterServiceClient<OtherTestOpenStackServiceClient>(new OtherTestOpenStackServiceClientDefinition());

            Assert.AreEqual(2, manager.serviceClientDefinitions.Count);
            Assert.IsTrue(manager.serviceClientDefinitions.ContainsKey(typeof(TestOpenStackServiceClient)));
            Assert.IsTrue(manager.serviceClientDefinitions.ContainsKey(typeof(OtherTestOpenStackServiceClient)));
        }

        [TestMethod]
        public void CanListAvailableClients()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.serviceClientDefinitions.Add(typeof(TestOpenStackServiceClient), new TestOpenStackServiceClientDefinition());
            manager.serviceClientDefinitions.Add(typeof(OtherTestOpenStackServiceClient), new OtherTestOpenStackServiceClientDefinition());

            var services = manager.ListAvailableServiceClients().ToList();

            Assert.AreEqual(2, services.Count());
            Assert.IsTrue(services.Contains(typeof(TestOpenStackServiceClient)));
            Assert.IsTrue(services.Contains(typeof(OtherTestOpenStackServiceClient)));
        }

        [TestMethod]
        public void CanCreateAClient()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.serviceClientDefinitions.Add(typeof(TestOpenStackServiceClient), new TestOpenStackServiceClientDefinition());

            var service = manager.CreateServiceClient<TestOpenStackServiceClient>(new OpenStackClientManagerTests.TestCredential(), CancellationToken.None);

            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(TestOpenStackServiceClient));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAServiceIfVersionIsNotSupported()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.serviceClientDefinitions.Add(typeof(OtherTestOpenStackServiceClient), new OtherTestOpenStackServiceClientDefinition());

            manager.CreateServiceClient<OtherTestOpenStackServiceClient>(new OpenStackClientManagerTests.TestCredential(), CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAClientIfNoServicesAreRegistered()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.CreateServiceClient<OtherTestOpenStackServiceClient>(new OpenStackClientManagerTests.TestCredential(), CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAServiceWithNullCredential()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.serviceClientDefinitions.Add(typeof(TestOpenStackServiceClient), new TestOpenStackServiceClientDefinition());

            manager.CreateServiceClient<TestOpenStackServiceClient>((ICredential)null, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAServiceWithNullCredentialAndVersion()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.serviceClientDefinitions.Add(typeof(TestOpenStackServiceClient), new TestOpenStackServiceClientDefinition());

            manager.CreateServiceClient<TestOpenStackServiceClient>((ICredential)null, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAServiceWithCredentialAndNullFactory()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.serviceClientDefinitions.Add(typeof(TestOpenStackServiceClient), null);

            manager.CreateServiceClient<TestOpenStackServiceClient>(new OpenStackClientManagerTests.TestCredential(), CancellationToken.None);
        }

        [TestMethod]
        public void CanCreateAnInstanceOfAService()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());

            var service = manager.CreateServiceClientInstance(new TestOpenStackServiceClientDefinition(), new OpenStackClientManagerTests.TestCredential(), string.Empty, CancellationToken.None);
            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(TestOpenStackServiceClient));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanCreateAnInstanceOfAServiceWithNullFactory()
        {
            var manager = new OpenStackServiceClientManager(new ServiceLocator());
            manager.CreateServiceClientInstance(null, new OpenStackClientManagerTests.TestCredential(), string.Empty, CancellationToken.None);
        }
    }
}
