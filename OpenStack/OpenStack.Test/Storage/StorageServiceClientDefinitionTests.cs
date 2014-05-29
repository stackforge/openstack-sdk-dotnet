using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common;
using OpenStack.Identity;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class StorageServiceClientDefinitionTests
    {
        IOpenStackCredential GetValidCreds()
        {
            var authId = "12345";
            var endpoint = "http://teststorageendpoint.com/v1/1234567890";

            var creds = new OpenStackCredential(new Uri(endpoint), "SomeUser", "Password", "SomeTenant");
            creds.SetAccessTokenId(authId);
            return creds;
        }

        [TestMethod]
        public void CanSupportVersion1()
        {
            var client = new StorageServiceClientDefinition();
            var creds = GetValidCreds();
            var catalog =
                new OpenStackServiceCatalog
                {
                    new OpenStackServiceDefinition("Swift", "Test",
                        new List<OpenStackServiceEndpoint>()
                        {
                            new OpenStackServiceEndpoint("http://someplace.com", "somewhere", "1.0",
                               "http://www.someplace.com", "http://www.someplace.com")
                        })
                };
            creds.SetServiceCatalog(catalog);
            Assert.IsTrue(client.IsSupported(creds, "Swift"));
        }

        [TestMethod]
        public void CannotSupportVersion2()
        {
            var client = new StorageServiceClientDefinition();
            var creds = GetValidCreds();
            var catalog =
                new OpenStackServiceCatalog
                {
                    new OpenStackServiceDefinition("Swift", "Test",
                        new List<OpenStackServiceEndpoint>()
                        {
                            new OpenStackServiceEndpoint("http://someplace.com", "somewhere", "2.0.0.0",
                                "http://www.someplace.com", "http://www.someplace.com")
                        })
                };
            creds.SetServiceCatalog(catalog);
            Assert.IsFalse(client.IsSupported(creds, "Swift"));
        }

        [TestMethod]
        public void CannotSupportUnknownServiceName()
        {
            var client = new StorageServiceClientDefinition();
            var creds = GetValidCreds();
            var catalog =
                new OpenStackServiceCatalog
                {
                    new OpenStackServiceDefinition("Swift", "Test",
                        new List<OpenStackServiceEndpoint>()
                        {
                            new OpenStackServiceEndpoint("http://someplace.com", "somewhere", "1.0",
                               "http://www.someplace.com", "http://www.someplace.com")
                        })
                };
            creds.SetServiceCatalog(catalog);
            Assert.IsFalse(client.IsSupported(creds, "BadServiceName"));
        }

        [TestMethod]
        public void Version1Supported()
        {
            var client = new StorageServiceClientDefinition();
            Assert.IsTrue(client.ListSupportedVersions().Contains("1.0"));
            Assert.IsTrue(client.ListSupportedVersions().Contains("1"));
        }
    }
}
