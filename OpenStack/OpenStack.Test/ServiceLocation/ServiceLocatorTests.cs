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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Test.ServiceLocation
{
    
    [TestClass]
    public class ServiceLocatorTests
    {
        public interface ITestEchoService
        {
            string Echo(string msg);
        }

        public class TestEchoService : ITestEchoService
        {
            public string Echo(string msg)
            {
                return msg;
            }
        }

        public class TestReverseEchoService : ITestEchoService
        {
            public string Echo(string msg)
            {
                return new string(msg.Reverse().ToArray());
            }
        }

        internal class TestServiceLocator : IServiceLocator
        {
            public T Locate<T>()
            {
                throw new NotImplementedException();
            }
        }

        internal class TestServiceManager : IServiceLocationRuntimeManager
        {
            public void RegisterServiceInstance<TService>(TService instance)
            {
                throw new NotImplementedException();
            }

            public void RegisterServiceInstance(Type type, object instance)
            {
                throw new NotImplementedException();
            }

            public void RegisterServiceType<T>(Type type)
            {
                throw new NotImplementedException();
            }

            public void RegisterServiceType<TInterface, TConcretion>() where TConcretion : class, TInterface
            {
                throw new NotImplementedException();
            }

            public void RegisterServiceType(Type type, Type registrationValue)
            {
                throw new NotImplementedException();
            }
        }

        internal class TestServiceOverrideManager : IServiceLocationOverrideManager
        {
            public void RegisterServiceInstance<TService>(TService instance)
            {
                throw new NotImplementedException();
            }

            public void RegisterServiceInstance(Type type, object instance)
            {
                throw new NotImplementedException();
            }

            public void RegisterServiceType<T>(Type type)
            {
                throw new NotImplementedException();
            }

            public void RegisterServiceType<TInterface, TConcretion>() where TConcretion : class, TInterface
            {
                throw new NotImplementedException();
            }

            public void RegisterServiceType(Type type, Type registrationValue)
            {
                throw new NotImplementedException();
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            ServiceLocator.Reset();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ServiceLocator.Reset();
        }

        [TestMethod]
        public void CanRegisterAndLocateAService()
        {
            var myServiceInstance = new TestEchoService();
            var manager = ServiceLocator.Instance.Locate<IServiceLocationRuntimeManager>();
            
            Assert.IsNotNull(manager);
            manager.RegisterServiceInstance<ITestEchoService>(myServiceInstance);

            var service = ServiceLocator.Instance.Locate<ITestEchoService>();
            
            Assert.IsNotNull(service);
            Assert.AreEqual("Works", service.Echo("Works"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotLocateAServiceThatHasNotBeenRegistered()
        {
            ServiceLocator.Instance.Locate<ITestEchoService>();
        }

        [TestMethod]
        public void CanOverrideAndLocateAService()
        {
            var echoServiceInstance = new TestEchoService();
            var reverseEchoServiceInstance = new TestReverseEchoService();
            var runtimeManager = ServiceLocator.Instance.Locate<IServiceLocationRuntimeManager>();
            var overrrideManager = ServiceLocator.Instance.Locate<IServiceLocationOverrideManager>();

            Assert.IsNotNull(runtimeManager);
            Assert.IsNotNull(overrrideManager);

            runtimeManager.RegisterServiceInstance<ITestEchoService>(echoServiceInstance);
            overrrideManager.RegisterServiceInstance<ITestEchoService>(reverseEchoServiceInstance);
            

            var service = ServiceLocator.Instance.Locate<ITestEchoService>();

            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(TestReverseEchoService));
            Assert.AreEqual("skroW", service.Echo("Works"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterNullService()
        {
            var runtimeManager = ServiceLocator.Instance.Locate<IServiceLocationRuntimeManager>();

            runtimeManager.RegisterServiceInstance<ITestEchoService>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterNullType()
        {
            var runtimeManager = ServiceLocator.Instance.Locate<IServiceLocationRuntimeManager>();

            runtimeManager.RegisterServiceInstance(null,new TestEchoService());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterNonInterface()
        {
            var runtimeManager = ServiceLocator.Instance.Locate<IServiceLocationRuntimeManager>();

            runtimeManager.RegisterServiceInstance<string>("Hello!");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterAServiceLocator()
        {
            var runtimeManager = ServiceLocator.Instance.Locate<IServiceLocationRuntimeManager>();

            runtimeManager.RegisterServiceInstance<IServiceLocator>(new TestServiceLocator());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterAServiceManager()
        {
            var runtimeManager = ServiceLocator.Instance.Locate<IServiceLocationRuntimeManager>();

            runtimeManager.RegisterServiceInstance<IServiceLocationRuntimeManager>(new TestServiceManager());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotRegisterAnOverrideManager()
        {
            var runtimeManager = ServiceLocator.Instance.Locate<IServiceLocationRuntimeManager>();

            runtimeManager.RegisterServiceInstance<IServiceLocationOverrideManager>(new TestServiceOverrideManager());
        }
    }
}
