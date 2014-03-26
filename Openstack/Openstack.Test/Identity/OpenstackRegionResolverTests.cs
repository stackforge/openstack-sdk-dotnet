using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Openstack.Identity;

namespace Openstack.Test.Identity
{
    [TestClass]
    public class OpenstackRegionResolverTests
    {
        [TestMethod]
        public void CanResolveRegion()
        {
            var expectedRegion = "some region";
            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", expectedRegion , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog.Add(new OpenstackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenstackRegionResolver();
            var region = resolver.Resolve(new Uri("http://other.endpoint.org/v2/tokens"), catalog, "Test Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        public void CannotResolveRegionWhenNoEnpointMatches()
        {
            var expectedRegion = string.Empty;
            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "Some Region" , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog.Add(new OpenstackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenstackRegionResolver();
            var region = resolver.Resolve(new Uri("http://nomatching.endpoint.org/v2/tokens"), catalog, "Test Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        public void CannotResolveRegionWhenNoMatchingServices()
        {
            var expectedRegion = string.Empty;
            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "Some Region" , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog.Add(new OpenstackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenstackRegionResolver();
            var region = resolver.Resolve(new Uri("http://nomatching.endpoint.org/v2/tokens"), catalog, "No Matching Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        public void CannotResolveRegionWhenMatchingEndpointHasEmptyRegion()
        {
            var expectedRegion = string.Empty;
            var catalog = new OpenstackServiceCatalog();
            catalog.Add(new OpenstackServiceDefinition("Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "Some Region" , "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));

            catalog[0].Endpoints.First().Region = string.Empty;

            catalog.Add(new OpenstackServiceDefinition("Other Test Service", "Test-Service",
                new List<OpenstackServiceEndpoint>()
                {
                    new OpenstackServiceEndpoint("http://other.endpoint.org", "some other region", "1.0",
                        "http://www.someplace.com", "http://www.someplace.com")
                }));
            var resolver = new OpenstackRegionResolver();
            var region = resolver.Resolve(new Uri("http://nomatching.endpoint.org/v2/tokens"), catalog, "Test Service");
            Assert.AreEqual(expectedRegion, region);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotResolveRegionWithNullCatalog()
        {
            var resolver = new OpenstackRegionResolver();
            resolver.Resolve(new Uri("http://other.endpoint.org/v2/tokens"), null, "Test Service");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotResolveRegionWithNullEndpoint()
        {
            var catalog = new OpenstackServiceCatalog();

            var resolver = new OpenstackRegionResolver();
            resolver.Resolve(null, catalog, "Test Service");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotResolveRegionWithNullServiceName()
        {
            var catalog = new OpenstackServiceCatalog();

            var resolver = new OpenstackRegionResolver();
            resolver.Resolve(new Uri("http://other.endpoint.org/v2/tokens"), catalog, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotResolveRegionWithEmptyServiceName()
        {
            var catalog = new OpenstackServiceCatalog();

            var resolver = new OpenstackRegionResolver();
            resolver.Resolve(new Uri("http://other.endpoint.org/v2/tokens"), catalog, string.Empty);
        }
    }
}
