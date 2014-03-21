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

namespace Openstack.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Openstack.Identity;

    [TestClass]
    public class OpenstackServiceClientManagerTests
    {
        internal class TestOpenstackServiceClient : IOpenstackServiceClient
        {
            public TestOpenstackServiceClient(ICredential credential, CancellationToken token)
            {
                
            }
        }

        internal class TestOpenstackServiceClientDefinition : IOpenstackServiceClientDefinition
        {
            public string Name { get; private set; }
            
            public IOpenstackServiceClient Create(ICredential credential, CancellationToken token)
            {
                return new TestOpenstackServiceClient(credential, token);
            }

            public IEnumerable<string> ListSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential)
            {
                return true;
            }
        }

        internal class OtherTestOpenstackServiceClient : IOpenstackServiceClient
        {
            public OtherTestOpenstackServiceClient(ICredential credential, CancellationToken token)
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

            public static IOpenstackServiceClient Create(ICredential credential, CancellationToken token)
            {
                return new OtherTestOpenstackServiceClient(credential, token);
            }
        }

        internal class OtherTestOpenstackServiceClientDefinition : IOpenstackServiceClientDefinition
        {
            public string Name { get; private set; }

            public IOpenstackServiceClient Create(ICredential credential, CancellationToken token)
            {
                return new OtherTestOpenstackServiceClient(credential, token);
            }

            public IEnumerable<string> ListSupportedVersions()
            {
                throw new NotImplementedException();
            }

            public bool IsSupported(ICredential credential)
            {
                return false;
            }
        }

        internal class NoValidCtroTestOpenstackServiceClient : IOpenstackServiceClient
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
            var manager = new OpenstackServiceClientManager();
            manager.RegisterServiceClient<TestOpenstackServiceClient>(new TestOpenstackServiceClientDefinition());

            Assert.AreEqual(1, manager.serviceClientDefinitions.Count);
            Assert.IsTrue(manager.serviceClientDefinitions.ContainsKey(typeof(TestOpenstackServiceClient)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterTheSameServiceTwice()
        {
            var manager = new OpenstackServiceClientManager();
            manager.RegisterServiceClient<TestOpenstackServiceClient>(new TestOpenstackServiceClientDefinition());
            manager.RegisterServiceClient<TestOpenstackServiceClient>(new TestOpenstackServiceClientDefinition());
        }

        [TestMethod]
        public void CanRegisterMultipleServices()
        {
            var manager = new OpenstackServiceClientManager();
            manager.RegisterServiceClient<TestOpenstackServiceClient>(new TestOpenstackServiceClientDefinition());
            manager.RegisterServiceClient<OtherTestOpenstackServiceClient>(new OtherTestOpenstackServiceClientDefinition());

            Assert.AreEqual(2, manager.serviceClientDefinitions.Count);
            Assert.IsTrue(manager.serviceClientDefinitions.ContainsKey(typeof(TestOpenstackServiceClient)));
            Assert.IsTrue(manager.serviceClientDefinitions.ContainsKey(typeof(OtherTestOpenstackServiceClient)));
        }

        [TestMethod]
        public void CanListAvailableClients()
        {
            var manager = new OpenstackServiceClientManager();
            manager.serviceClientDefinitions.Add(typeof(TestOpenstackServiceClient), new TestOpenstackServiceClientDefinition());
            manager.serviceClientDefinitions.Add(typeof(OtherTestOpenstackServiceClient), new OtherTestOpenstackServiceClientDefinition());

            var services = manager.ListAvailableServiceClients().ToList();

            Assert.AreEqual(2, services.Count());
            Assert.IsTrue(services.Contains(typeof(TestOpenstackServiceClient)));
            Assert.IsTrue(services.Contains(typeof(OtherTestOpenstackServiceClient)));
        }

        [TestMethod]
        public void CanCreateAClient()
        {
            var manager = new OpenstackServiceClientManager();
            manager.serviceClientDefinitions.Add(typeof(TestOpenstackServiceClient), new TestOpenstackServiceClientDefinition());

            var service = manager.CreateServiceClient<TestOpenstackServiceClient>(new OpenstackClientManagerTests.TestCredential(), CancellationToken.None);

            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(TestOpenstackServiceClient));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAServiceIfVersionIsNotSupported()
        {
            var manager = new OpenstackServiceClientManager();
            manager.serviceClientDefinitions.Add(typeof(OtherTestOpenstackServiceClient), new OtherTestOpenstackServiceClientDefinition());

            manager.CreateServiceClient<OtherTestOpenstackServiceClient>(new OpenstackClientManagerTests.TestCredential(), CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAClientIfNoServicesAreRegistered()
        {
            var manager = new OpenstackServiceClientManager();
            manager.CreateServiceClient<OtherTestOpenstackServiceClient>(new OpenstackClientManagerTests.TestCredential(), CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAServiceWithNullCredential()
        {
            var manager = new OpenstackServiceClientManager();
            manager.serviceClientDefinitions.Add(typeof(TestOpenstackServiceClient), new TestOpenstackServiceClientDefinition());

            manager.CreateServiceClient<TestOpenstackServiceClient>((ICredential)null, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAServiceWithNullCredentialAndVersion()
        {
            var manager = new OpenstackServiceClientManager();
            manager.serviceClientDefinitions.Add(typeof(TestOpenstackServiceClient), new TestOpenstackServiceClientDefinition());

            manager.CreateServiceClient<TestOpenstackServiceClient>((ICredential)null, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCreateAServiceWithCredentialAndNullFactory()
        {
            var manager = new OpenstackServiceClientManager();
            manager.serviceClientDefinitions.Add(typeof(TestOpenstackServiceClient), null);

            manager.CreateServiceClient<TestOpenstackServiceClient>(new OpenstackClientManagerTests.TestCredential(), CancellationToken.None);
        }

        [TestMethod]
        public void CanCreateAnInstanceOfAService()
        {
            var manager = new OpenstackServiceClientManager();

            var service = manager.CreateServiceClientInstance(new TestOpenstackServiceClientDefinition(), new OpenstackClientManagerTests.TestCredential(), CancellationToken.None);
            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(TestOpenstackServiceClient));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanCreateAnInstanceOfAServiceWithNullFactory()
        {
            var manager = new OpenstackServiceClientManager();
            manager.CreateServiceClientInstance(null, new OpenstackClientManagerTests.TestCredential(), CancellationToken.None);
        }
    }
}
