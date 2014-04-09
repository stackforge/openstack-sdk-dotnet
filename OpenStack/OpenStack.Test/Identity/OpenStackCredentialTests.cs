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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class OpenStackCredentialTests
    {
        [TestMethod]
        public void CanCreateAnOpenStackCredential()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            var cred = new OpenStackCredential(endpoint, userName, password, tenantId);
            
            Assert.IsNotNull(cred);
            Assert.AreEqual(userName, cred.UserName);
            Assert.AreEqual(endpoint, cred.AuthenticationEndpoint);
            Assert.IsNotNull(cred.Password);
            Assert.AreEqual(tenantId, cred.TenantId);
        }

        [TestMethod]
        public void CanSetAcessTokenId()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";
            var token = "someToken";

            var cred = new OpenStackCredential(endpoint, userName, password, tenantId);
            cred.SetAccessTokenId(token);

            Assert.AreEqual(token, cred.AccessTokenId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotSetAcessTokenIdWithNullToken()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            var cred = new OpenStackCredential(endpoint, userName, password, tenantId);
            cred.SetAccessTokenId(null);
        }

        [TestMethod]
        public void CanSetServiceCatalog()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            var catalog = new OpenStackServiceCatalog();

            var cred = new OpenStackCredential(endpoint, userName, password, tenantId);
            cred.SetServiceCatalog(catalog);

            Assert.AreEqual(catalog, cred.ServiceCatalog);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotSetServiceCatalogWithNullToken()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            var cred = new OpenStackCredential(endpoint, userName, password, tenantId);
            cred.SetServiceCatalog(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotSetAcessTokenIdWithEmptyToken()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            var cred = new OpenStackCredential(endpoint, userName, password, tenantId);
            cred.SetAccessTokenId(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAnOpenStackCredentialWithNullEndpoint()
        {
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            var cred = new OpenStackCredential(null, userName, password, tenantId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAnOpenStackCredentialWithNullUserName()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var password = "RandomPassword";
            var tenantId = "12345";

            var cred = new OpenStackCredential(endpoint, null, password, tenantId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotCreateAnOpenStackCredentialWithEmptyUserName()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var password = "RandomPassword";
            var tenantId = "12345";

            var cred = new OpenStackCredential(endpoint, string.Empty, password, tenantId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAnOpenStackCredentialWithNullPassword()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var tenantId = "12345";

            var cred = new OpenStackCredential(endpoint, userName, null, tenantId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotCreateAnOpenStackCredentialWithNullTenantId()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";

            var cred = new OpenStackCredential(endpoint, userName, password, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotCreateAnOpenStackCredentialWithEmptyTenantId()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";

            var cred = new OpenStackCredential(endpoint, userName, password, string.Empty);
        }
    }
}
