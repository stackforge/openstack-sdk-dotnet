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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Openstack.Common.ServiceLocation;

namespace Openstack.Objects.Test.ServiceLocation
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
        public void HasNewAssembliesWithNewAssemblies()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var temp = assemblies.First();
            assemblies.Remove(temp);
            
            sweeper.GetAllAssemblies = () => assemblies;
            
            Assert.IsTrue(sweeper.HasNewAssemblies());
            Assert.IsFalse(sweeper.HasNewAssemblies());
            assemblies.Add(temp);
            
            Assert.IsTrue(sweeper.HasNewAssemblies());
            Assert.IsFalse(sweeper.HasNewAssemblies());
        }

        [TestMethod]
        public void CanGetOnlyRegistrarTypes()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = new List<Assembly>() {this.GetType().Assembly};

            sweeper.GetAllAssemblies = () => assemblies;
            var types = sweeper.GetRegistrarTypes();
            Assert.AreEqual(3, types.Count());
        }

        [TestMethod]
        public void OnlyRegistrarTypesAreReturned()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            assemblies.Remove(this.GetType().Assembly);

            sweeper.GetAllAssemblies = () => assemblies;
            var types = sweeper.GetRegistrarTypes();
            Assert.AreEqual(2, types.Count());
        }

        [TestMethod] 
        public void CanGetOnlyRegistrars()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            assemblies.Remove(this.GetType().Assembly);

            sweeper.GetAllAssemblies = () => assemblies;
            var registrars = sweeper.GetRegistrars();
            Assert.AreEqual(2, registrars.Count());
        }

        [TestMethod]
        public void CanGetNewRegistrars()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            assemblies.RemoveAll( a => true);

            sweeper.GetAllAssemblies = () => assemblies;

            var registrars = sweeper.GetRegistrars();
            Assert.AreEqual(0, registrars.Count());

            assemblies = new List<Assembly>() { this.GetType().Assembly };
            
            registrars = sweeper.GetRegistrars();
            Assert.AreEqual(3, registrars.Count());

        }


        [TestMethod]
        public void CanGetOnlyRegistrar()
        {
            var sweeper = new ServiceLocationAssemblyScanner();
            var assemblies = new List<Assembly>() { this.GetType().Assembly };

            sweeper.GetAllAssemblies = () => assemblies;
            var registrars = sweeper.GetRegistrars();
            Assert.AreEqual(3, registrars.Count());
        }
    }
}
