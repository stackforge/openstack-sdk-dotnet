using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class IdentityServiceClientDefinitionTests
    {
        public IOpenStackCredential GetValidCredentials()
        {
            var endpoint = new Uri("https://someidentityendpoint:35357/v2.0");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            return new OpenStackCredential(endpoint, userName, password, tenantId);
        }

        [TestMethod]
        public void CanSupportVersion2()
        {
            var client = new IdentityServiceClientDefinition();

            Assert.IsTrue(client.IsSupported(GetValidCredentials(), string.Empty));
        }

        [TestMethod]
        public void CannotSupportVersion1()
        {
            var endpoint = new Uri("https://someidentityendpoint:35357/v1.0/tokens");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            var creds = new OpenStackCredential(endpoint, userName, password, tenantId);

            var client = new IdentityServiceClientDefinition();
            
            Assert.IsFalse(client.IsSupported(creds, string.Empty));
        }

        [TestMethod]
        public void Version2Supported()
        {
            var client = new IdentityServiceClientDefinition();
            Assert.IsTrue(client.ListSupportedVersions().Contains("2.0.0.0"));
        }
    }
}
