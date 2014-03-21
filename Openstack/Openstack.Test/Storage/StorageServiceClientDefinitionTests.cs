using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Openstack.Common;
using Openstack.Identity;
using Openstack.Storage;

namespace Openstack.Test.Storage
{
    [TestClass]
    public class StorageServiceClientDefinitionTests
    {
        IOpenstackCredential GetValidCreds()
        {
            var authId = "12345";
            var endpoint = "http://teststorageendpoint.com/v1/1234567890";

            var creds = new OpenstackCredential(new Uri(endpoint), "SomeUser", "Password".ConvertToSecureString(), "SomeTenant");
            creds.SetAccessTokenId(authId);
            return creds;
        }

        [TestMethod]
        public void CanSupportVersion1()
        {
            var client = new StorageServiceClientDefinition();
            var creds = GetValidCreds();
            var catalog =
                new OpenstackServiceCatalog
                {
                    new OpenstackServiceDefinition("Object Storage", "Test",
                        new List<OpenstackServiceEndpoint>()
                        {
                            new OpenstackServiceEndpoint("http://someplace.com", "somewhere", "1.0",
                                new Uri("http://someplace.com"), new Uri("http://someplace.com"))
                        })
                };
            creds.SetServiceCatalog(catalog);
            Assert.IsTrue(client.IsSupported(creds));
        }

        [TestMethod]
        public void CannotSupportVersion2()
        {
            var client = new StorageServiceClientDefinition();
            var creds = GetValidCreds();
            var catalog =
                new OpenstackServiceCatalog
                {
                    new OpenstackServiceDefinition("Object Storage", "Test",
                        new List<OpenstackServiceEndpoint>()
                        {
                            new OpenstackServiceEndpoint("http://someplace.com", "somewhere", "2.0.0.0",
                                new Uri("http://someplace.com"), new Uri("http://someplace.com"))
                        })
                };
            creds.SetServiceCatalog(catalog);
            Assert.IsFalse(client.IsSupported(creds));
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
