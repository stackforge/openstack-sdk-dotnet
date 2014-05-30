using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class OpenStackRegionResolverTests
    {
        [TestMethod]
        public void CanResolveRegion()
        {
            var expectedRegion = "some region";
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", expectedRegion , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog.Add(new OpenStackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenStackRegionResolver();
            var region = resolver.Resolve(new Uri("http://other.endpoint.org/v2/tokens"), catalog, "Test Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        public void CannotResolveRegionWhenNoEnpointMatches()
        {
            var expectedRegion = string.Empty;
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "Some Region" , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog.Add(new OpenStackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenStackRegionResolver();
            var region = resolver.Resolve(new Uri("http://nomatching.endpoint.org/v2/tokens"), catalog, "Test Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        public void CannotResolveRegionWhenNoMatchingServices()
        {
            var expectedRegion = string.Empty;
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "Some Region" , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog.Add(new OpenStackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenStackRegionResolver();
            var region = resolver.Resolve(new Uri("http://nomatching.endpoint.org/v2/tokens"), catalog, "No Matching Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        public void CanResolveRegionWhenNoMatchingServicesButMatchingEndpoint()
        {
            var expectedRegion = "Some Region";
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://the.endpoint.org", "Some Region" , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog.Add(new OpenStackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenStackRegionResolver();
            var region = resolver.Resolve(new Uri("http://the.endpoint.org/v2/tokens"), catalog, "No Matching Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        public void CannotResolveRegionWhenMatchingEndpointHasEmptyRegion()
        {
            var expectedRegion = string.Empty;
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "Some Region" , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog[0].Endpoints.First().Region = string.Empty;

            catalog.Add(new OpenStackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenStackRegionResolver();
            var region = resolver.Resolve(new Uri("http://nomatching.endpoint.org/v2/tokens"), catalog, "Test Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotResolveRegionWithNullCatalog()
        {
            var resolver = new OpenStackRegionResolver();
            resolver.Resolve(new Uri("http://other.endpoint.org/v2/tokens"), null, "Test Service");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotResolveRegionWithNullEndpoint()
        {
            var catalog = new OpenStackServiceCatalog();

            var resolver = new OpenStackRegionResolver();
            resolver.Resolve(null, catalog, "Test Service");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotResolveRegionWithNullServiceName()
        {
            var catalog = new OpenStackServiceCatalog();

            var resolver = new OpenStackRegionResolver();
            resolver.Resolve(new Uri("http://other.endpoint.org/v2/tokens"), catalog, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotResolveRegionWithEmptyServiceName()
        {
            var catalog = new OpenStackServiceCatalog();

            var resolver = new OpenStackRegionResolver();
            resolver.Resolve(new Uri("http://other.endpoint.org/v2/tokens"), catalog, string.Empty);
        }
    }
}
