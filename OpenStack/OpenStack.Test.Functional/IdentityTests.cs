// /* ============================================================================
// Copyright 2014 Hewlett Packard
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Identity;

namespace OpenStack.Test.Functional
{
    /// <summary>
    /// Tests for the Identity Component
    /// </summary>
    [TestClass]
    public class IdentityTests
    {
        /// <summary>
        /// Verifies that we can connect to the API. A successful connection is determined by whether or not
        /// a token was returned. The token is used for future API calls.
        /// </summary>
        [TestMethod]
        public void CanConnect()
        {
            var credential = new OpenStackCredential(Configuration.AuthUri, Configuration.UserName, Configuration.Password, Configuration.TenantId);
            var client = OpenStackClientFactory.CreateClient(credential);

            client.Connect().Wait();

            Assert.IsNotNull(credential.AccessTokenId, "No AccessTokenId was set, authentication failed.");
            Assert.AreNotEqual(0, credential.ServiceCatalog.Count, "ServiceCatalog should not be empty.");
        }
    }
}
