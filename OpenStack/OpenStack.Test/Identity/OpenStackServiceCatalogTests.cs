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
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class OpenStackServiceCatalogTests
    {
        [TestMethod]
        public void IfAServiceExistsCatalogReturnsTrue()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                });
            var catalog = new OpenStackServiceCatalog { serviceDef };
            Assert.IsTrue(catalog.Exists("Test Service"));
        }

        [TestMethod]
        public void IfAServiceDoesNotExistsCatalogReturnsFalse()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                });
            var catalog = new OpenStackServiceCatalog { serviceDef };
            Assert.IsFalse(catalog.Exists("NOT IN CATALOG"));
        }

        [TestMethod]
        public void CanGetServicesInRegion()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                });
            var catalog = new OpenStackServiceCatalog { serviceDef };
            var services = catalog.GetServicesInAvailabilityZone("some region").ToList();
            Assert.AreEqual(1,services.Count());
            Assert.AreEqual("Test Service",services.First().Name);
        }

        [TestMethod]
        public void CanGetServicesInRegionWithMultipleServices()
        {
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://some.endpoint.com", "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog.Add(new OpenStackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            var services = catalog.GetServicesInAvailabilityZone("some region").ToList();
            Assert.AreEqual(2, services.Count());
        }

        [TestMethod]
        public void GettingServicesInRegionWhenNonExistReturnsAnEmptyCollection()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                });
            var catalog = new OpenStackServiceCatalog { serviceDef };
            var services = catalog.GetServicesInAvailabilityZone("some other region").ToList();
            Assert.AreEqual(0, services.Count());
        }

        [TestMethod]
        public void CanGetServicesForRegionWithMultipleEndpoints()
        {
            var expectedEndpoint = new Uri("http://public.endpoint.org");
            var serviceDef = new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(expectedEndpoint.ToString(), "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com"),
                    new OpenStackServiceEndpoint(expectedEndpoint.ToString(), "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                });
            var catalog = new OpenStackServiceCatalog { serviceDef };
            var services = catalog.GetServicesInAvailabilityZone("some other region").ToList();
            Assert.AreEqual(1, services.Count());
        }

        [TestMethod]
        public void CanGetPublicEndpointForAService()
        {
            var serviceName = "Test Service";
            var regionName = "some region";
            var expectedEndpoint = "http://public.endpoint.org";
            var serviceDef = new OpenStackServiceDefinition(serviceName, "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(expectedEndpoint, "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                });

            var catalog = new OpenStackServiceCatalog { serviceDef };
            var endpoint = catalog.GetPublicEndpoint(serviceName, regionName);
            Assert.AreEqual(expectedEndpoint, endpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotGetPublicEndpointWithANullServiceName()
        {
            var catalog = new OpenStackServiceCatalog();
            catalog.GetPublicEndpoint(null, "Some region");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotGetPublicEndpointWithANullRegion()
        {
            var catalog = new OpenStackServiceCatalog();
            catalog.GetPublicEndpoint("Test Service", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotGetPublicEndpointWithAnEmptyServiceName()
        {
            var catalog = new OpenStackServiceCatalog();
            catalog.GetPublicEndpoint(string.Empty, "Some region");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotGetPublicEndpointForAServiceThatIsNotInTheCatalog()
        {
            var serviceName = "Test Service";
            var regionName = "some region";
            var expectedEndpoint = "http://public.endpoint.org";
            var serviceDef = new OpenStackServiceDefinition(serviceName, "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(expectedEndpoint, "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                });

            var catalog = new OpenStackServiceCatalog { serviceDef };
            catalog.GetPublicEndpoint("Not-In-Catalog", regionName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotGetPublicEndpointForAServiceThatIsNotInTheRegion()
        {
            var serviceName = "Test Service";
            var expectedEndpoint = "http://public.endpoint.org";
            var serviceDef = new OpenStackServiceDefinition(serviceName, "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(expectedEndpoint, "some region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                });

            var catalog = new OpenStackServiceCatalog { serviceDef };
            catalog.GetPublicEndpoint(serviceName, "Not-in-catalog");
        }
    }
}
