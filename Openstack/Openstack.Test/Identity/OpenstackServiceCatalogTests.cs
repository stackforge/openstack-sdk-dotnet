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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Openstack.Identity;

namespace Openstack.Test.Identity
{
    [TestClass]
    public class OpenstackServiceCatalogTests
    {
        [TestMethod]
        public void CanGetPublicEndpointForService()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                });
            var catalog = new OpenstackServiceCatalog {serviceDef};
            var endpoint = catalog.GetPublicEndpoint("Test Service", "some region");
            Assert.AreEqual(expectedEndpoint,endpoint);
        }

        [TestMethod]
        public void CanGetPublicEndpointForServiceWithMultipleServicesInCatalog()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                }));

            catalog.Add(new OpenstackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some region", "1.0",
                        new Uri("http://www.someotherplace.com"), new Uri("http://www.someplace.com"))
                }));
           
            var endpoint = catalog.GetPublicEndpoint("Test Service", "some region");
            Assert.AreEqual(expectedEndpoint, endpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotGetPublicEndpointWithNoMatchingService()
        {
            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://some.endpoint.org", "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                }));

            catalog.Add(new OpenstackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some region", "1.0",
                        new Uri("http://www.someotherplace.com"), new Uri("http://www.someplace.com"))
                }));

            catalog.GetPublicEndpoint("Missing Service", "some region");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotGetPublicEndpointWithServiceAndNoEndpoints()
        {
            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()));

            catalog.Add(new OpenstackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some region", "1.0",
                        new Uri("http://www.someotherplace.com"), new Uri("http://www.someplace.com"))
                }));

            catalog.GetPublicEndpoint("Test Service", "some region");
        }

        [TestMethod]
        public void CanGetPublicEndpointWithServiceAndMultipleEndpoints()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");

            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://public.endpoint.org", "some region", "1.0",
                        new Uri("http://www.someotherplace.com"), new Uri("http://www.someplace.com")),

                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        new Uri("http://www.someotherplace.com"), new Uri("http://www.someplace.com"))
                }));

            var endpoint = catalog.GetPublicEndpoint("Test Service", "some region");
            Assert.AreEqual(expectedEndpoint, endpoint);
        }

        [TestMethod]
        public void IfAServiceExistsCatalogReturnsTrue()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                });
            var catalog = new OpenstackServiceCatalog { serviceDef };
            Assert.IsTrue(catalog.Exists("Test Service"));
        }

        [TestMethod]
        public void IfAServiceDoesNotExistsCatalogReturnsFalse()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                });
            var catalog = new OpenstackServiceCatalog { serviceDef };
            Assert.IsFalse(catalog.Exists("NOT IN CATALOG"));
        }

        [TestMethod]
        public void CanGetServicesInRegion()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                });
            var catalog = new OpenstackServiceCatalog { serviceDef };
            var services = catalog.GetServicesInAvailabilityZone("some region").ToList();
            Assert.AreEqual(1,services.Count());
            Assert.AreEqual("Test Service",services.First().Name);
        }

        [TestMethod]
        public void CanGetServicesInRegionWithMultipleServices()
        {
            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://some.endpoint.com", "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                }));

            catalog.Add(new OpenstackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some region", "1.0",
                        new Uri("http://www.someotherplace.com"), new Uri("http://www.someplace.com"))
                }));

            var services = catalog.GetServicesInAvailabilityZone("some region").ToList();
            Assert.AreEqual(2, services.Count());
        }

        [TestMethod]
        public void GettingServicesInRegionWhenNonExistReturnsAnEmptyCollection()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                });
            var catalog = new OpenstackServiceCatalog { serviceDef };
            var services = catalog.GetServicesInAvailabilityZone("some other region").ToList();
            Assert.AreEqual(0, services.Count());
        }

        [TestMethod]
        public void CanGetServicesForRegionWithMultipleEndpoints()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com")),
                    new OpenstackServiceEndpoint(expectedEndpoint.ToString(), "some other region", "1.0",
                        new Uri("http://www.someplace.com"), new Uri("http://www.someplace.com"))
                });
            var catalog = new OpenstackServiceCatalog { serviceDef };
            var services = catalog.GetServicesInAvailabilityZone("some other region").ToList();
            Assert.AreEqual(1, services.Count());
        }
    }
}
