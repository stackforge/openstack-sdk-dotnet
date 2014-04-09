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
using System.Reflection;
using System.Web.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Test.ServiceLocation
{
    [TestClass]
    public class ServiceLocationAssemblyScannerTests
    {
        internal class TestRegistrar : IServiceLocationRegistrar
        {
            public void Register(IServiceLocationManager manager, IServiceLocator locator)
            {
                
            }
        }

        internal class OtherTestRegistrar : IServiceLocationRegistrar
        {
            public void Register(IServiceLocationManager manager, IServiceLocator locator)
            {
                
            }
        }

        internal class NonDefaultTestRegistrar : IServiceLocationRegistrar
        {
            public NonDefaultTestRegistrar(string beans)
            {
                //this is here to force a non-default constructor, this should not be loaded as a registrar. 
            }

            public void Register(IServiceLocationManager manager, IServiceLocator locator)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void CanAddNewAssembly()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var temp = assemblies.First();
            
            Assert.IsFalse(sweeper.HasNewAssemblies);
            
            sweeper.AddAssembly(temp);
            
            Assert.IsTrue(sweeper.HasNewAssemblies);
            Assert.AreEqual(1,sweeper._assemblies.Count);
        }

        [TestMethod]
        public void CanAddExistingAssembly()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var temp = assemblies.First();

            Assert.IsFalse(sweeper.HasNewAssemblies);
            sweeper.AddAssembly(temp);
            Assert.IsTrue(sweeper.HasNewAssemblies);
            
            sweeper.AddAssembly(temp);
           
            Assert.IsTrue(sweeper.HasNewAssemblies);
            Assert.AreEqual(1, sweeper._assemblies.Count);
        }

        [TestMethod]
        public void CanGetRegistrarTypes()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            Assert.IsFalse(sweeper.HasNewAssemblies);
            
            sweeper.AddAssembly(this.GetType().Assembly);
            Assert.IsTrue(sweeper.HasNewAssemblies);
            
            var types = sweeper.GetRegistrarTypes().ToList();
            Assert.IsTrue(sweeper.HasNewAssemblies);
            Assert.AreEqual(2, types.Count());
            Assert.IsTrue(types.Contains(typeof(TestRegistrar)));
            Assert.IsTrue(types.Contains(typeof(OtherTestRegistrar)));
        }

        [TestMethod]
        public void CanGetRegistrarTypesWithAssemblyThatHasNoRegistrars()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            Assert.IsFalse(sweeper.HasNewAssemblies);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var temp = assemblies.First();

            sweeper.AddAssembly(temp);
            Assert.IsTrue(sweeper.HasNewAssemblies);

            var types = sweeper.GetRegistrarTypes().ToList();
            Assert.IsTrue(sweeper.HasNewAssemblies);

            Assert.AreEqual(0, types.Count());
        }

        [TestMethod]
        public void CanGetRegistrarTypesWithEmptyAssembliesCollection()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            Assert.IsFalse(sweeper.HasNewAssemblies);

            var types = sweeper.GetRegistrarTypes().ToList();
            Assert.IsFalse(sweeper.HasNewAssemblies);

            Assert.AreEqual(0, types.Count());
        }

        [TestMethod]
        public void NoNewRegistrarsIfNoNewAssemblies()
        {
            bool getRegistrarsCalled = false;
            var sweeper = new ServiceLocationAssemblyScanner();
            sweeper.GetRegistrarTypes = () => { getRegistrarsCalled = false;
                                                  return new List<Type>();
            };

            var regs = sweeper.GetRegistrars();
            Assert.IsFalse(getRegistrarsCalled);
            Assert.AreEqual(0, regs.Count());
        }

        [TestMethod]
        public void NewRegistrarsIfNewAssembliesPresent()
        {
            bool getRegistrarsCalled = false;
            var sweeper = new ServiceLocationAssemblyScanner();
            sweeper.HasNewAssemblies = true;
            sweeper.GetRegistrarTypes = () =>
            {
                getRegistrarsCalled = true;
                return new List<Type>() { typeof(TestRegistrar)};
            };

            var regs = sweeper.GetRegistrars().ToList();
            Assert.IsTrue(getRegistrarsCalled);
            Assert.AreEqual(1, regs.Count());
            Assert.IsTrue(regs.First() is TestRegistrar);
        }

        [TestMethod]
        public void ExistingRegistrarIsOverwritenWhenNewAssemblyFoundThatContainsIt()
        {
            bool getRegistrarsCalled = false;
            var sweeper = new ServiceLocationAssemblyScanner();
            sweeper._registrars.Add(typeof(TestRegistrar));
            sweeper.HasNewAssemblies = true;
            sweeper.GetRegistrarTypes = () =>
            {
                getRegistrarsCalled = true;
                return new List<Type>() { typeof(TestRegistrar) };
            };

            var regs = sweeper.GetRegistrars().ToList();
            Assert.IsTrue(getRegistrarsCalled);
            Assert.AreEqual(1, regs.Count());
            Assert.IsTrue(regs.First() is TestRegistrar);
            Assert.AreEqual(1, sweeper._registrars.Count);
            Assert.AreEqual(typeof(TestRegistrar), sweeper._registrars.First());
        }

        [TestMethod] 
        public void CanGetOnlyRegistrars()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            assemblies.Remove(this.GetType().Assembly);
            assemblies.ForEach(sweeper.AddAssembly);
            Assert.IsTrue(sweeper.HasNewAssemblies);

            var registrars = sweeper.GetRegistrars();
            Assert.IsFalse(sweeper.HasNewAssemblies);
            Assert.AreEqual(1, registrars.Count());
        }

        [TestMethod]
        public void CanGetNewRegistrars()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            Assert.IsFalse(sweeper.HasNewAssemblies);

            var registrars = sweeper.GetRegistrars();
            Assert.IsFalse(sweeper.HasNewAssemblies);
            Assert.AreEqual(0, registrars.Count());

           sweeper.AddAssembly( this.GetType().Assembly );
           Assert.IsTrue(sweeper.HasNewAssemblies);
            
            registrars = sweeper.GetRegistrars();
            Assert.IsFalse(sweeper.HasNewAssemblies);
            Assert.AreEqual(2, registrars.Count());
        }
    }
}
