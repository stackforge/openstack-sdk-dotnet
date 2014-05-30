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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class IdentityServicePocoClientTests
    {
        internal TestIdentityServiceRestClient RestClient;
        internal string defaultRegion = "region-a.geo-1";
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.RestClient = new TestIdentityServiceRestClient();
            this.ServiceLocator = new ServiceLocator();
            this.ServiceLocator.EnsureAssemblyRegistration(typeof(ServiceLocator).Assembly);

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IIdentityServiceRestClientFactory), new TestIdentityServiceRestClientFactory(RestClient));
            manager.RegisterServiceInstance(typeof(IOpenStackRegionResolver), new TestOpenStackRegionResolver(defaultRegion));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.RestClient = new TestIdentityServiceRestClient();
            this.ServiceLocator = new ServiceLocator();
        }

        public IOpenStackCredential GetValidCredentials()
        {
            var endpoint = new Uri("https://auth.someplace.com/authme");
            var userName = "TestUser";
            var password = "RandomPassword";
            var tenantId = "12345";

            return new OpenStackCredential(endpoint, userName, password, tenantId);
        }


        [TestMethod]
        public async Task CanAuthenticateWithOkResponse()
        {
            var creds = GetValidCredentials();

            var expectedToken = "HPAuth10_af3d1bfe456d18e8d4793e54922f839fa051d9f60f115aca52c9a44f9e3d96fb";

            var payload = @"{
                                                ""access"": {
                                                    ""token"": {
                                                        ""expires"": ""2014-03-18T10:59:46.355Z"",
                                                        ""id"": ""HPAuth10_af3d1bfe456d18e8d4793e54922f839fa051d9f60f115aca52c9a44f9e3d96fb"",
                                                        ""tenant"": {
                                                            ""id"": ""10244656540440"",
                                                            ""name"": ""10255892528404-Project""
                                                        }
                                                    },
                                                    ""serviceCatalog"":[{
                                                        ""name"": ""Object Storage"",
                                                        ""type"": ""object-store"",
                                                        ""endpoints"": [
                                                            {
                                                                ""tenantId"": ""10244656540440"",
                                                                ""publicURL"": ""https://region-a.geo-1.objects.hpcloudsvc.com/v1/10244656540440"",
                                                                ""region"": ""region-a.geo-1"",
                                                                ""versionId"": ""1.0"",
                                                                ""versionInfo"": ""https://region-a.geo-1.objects.hpcloudsvc.com/v1.0/"",
                                                                ""versionList"": ""https://region-a.geo-1.objects.hpcloudsvc.com""
                                                            },
                                                            {
                                                                ""tenantId"": ""10244656540440"",
                                                                ""publicURL"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443/v1/10244656540440"",
                                                                ""region"": ""region-b.geo-1"",
                                                                ""versionId"": ""1"",
                                                                ""versionInfo"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443/v1/"",
                                                                ""versionList"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443""
                                                            }
                                                        ]
                                                    }]
                                                }
                                            }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.OK);
            this.RestClient.Response = restResp;

            var client =
                new IdentityServicePocoClientFactory().Create(creds, "Swift", CancellationToken.None, this.ServiceLocator) as
                    IdentityServicePocoClient;
            var result = await client.Authenticate();

            Assert.IsNotNull(result);
            Assert.AreEqual(creds.UserName, result.UserName);
            Assert.AreEqual(creds.Password, result.Password);
            Assert.AreEqual(creds.TenantId, result.TenantId);
            Assert.AreEqual(expectedToken, result.AccessTokenId);
            Assert.AreEqual(1,result.ServiceCatalog.Count());
        }

        [TestMethod]
        public async Task CanAuthenticateWithNonAuthoritativeInfoResponse()
        {
            var creds = GetValidCredentials();

            var expectedToken = "HPAuth10_af3d1bfe456d18e8d4793e54922f839fa051d9f60f115aca52c9a44f9e3d96fb";

            var payload = @"{
                                                ""access"": {
                                                    ""token"": {
                                                        ""expires"": ""2014-03-18T10:59:46.355Z"",
                                                        ""id"": ""HPAuth10_af3d1bfe456d18e8d4793e54922f839fa051d9f60f115aca52c9a44f9e3d96fb"",
                                                        ""tenant"": {
                                                            ""id"": ""10244656540440"",
                                                            ""name"": ""10255892528404-Project""
                                                        }
                                                    },
                                                    ""serviceCatalog"":[{
                                                        ""name"": ""Object Storage"",
                                                        ""type"": ""object-store"",
                                                        ""endpoints"": [
                                                            {
                                                                ""tenantId"": ""10244656540440"",
                                                                ""publicURL"": ""https://region-a.geo-1.objects.hpcloudsvc.com/v1/10244656540440"",
                                                                ""region"": ""region-a.geo-1"",
                                                                ""versionId"": ""1.0"",
                                                                ""versionInfo"": ""https://region-a.geo-1.objects.hpcloudsvc.com/v1.0/"",
                                                                ""versionList"": ""https://region-a.geo-1.objects.hpcloudsvc.com""
                                                            },
                                                            {
                                                                ""tenantId"": ""10244656540440"",
                                                                ""publicURL"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443/v1/10244656540440"",
                                                                ""region"": ""region-b.geo-1"",
                                                                ""versionId"": ""1"",
                                                                ""versionInfo"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443/v1/"",
                                                                ""versionList"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443""
                                                            }
                                                        ]
                                                    }]
                                                }
                                            }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            var result = await client.Authenticate();

            Assert.IsNotNull(result);
            Assert.AreEqual(creds.UserName, result.UserName);
            Assert.AreEqual(creds.Password, result.Password);
            Assert.AreEqual(creds.TenantId, result.TenantId);
            Assert.AreEqual(expectedToken, result.AccessTokenId);
            Assert.AreEqual(1, result.ServiceCatalog.Count());
        }

        [TestMethod]
        public async Task AuthenticationDoesNotResolveRegionIfCredRegionSupplied()
        {
            var expectedRegion = "Some region";
            var creds = GetValidCredentials();
            creds.SetRegion(expectedRegion);
            var payload = @"{
                                                ""access"": {
                                                    ""token"": {
                                                        ""expires"": ""2014-03-18T10:59:46.355Z"",
                                                        ""id"": ""HPAuth10_af3d1bfe456d18e8d4793e54922f839fa051d9f60f115aca52c9a44f9e3d96fb"",
                                                        ""tenant"": {
                                                            ""id"": ""10244656540440"",
                                                            ""name"": ""10255892528404-Project""
                                                        }
                                                    },
                                                    ""serviceCatalog"":[{
                                                        ""name"": ""Object Storage"",
                                                        ""type"": ""object-store"",
                                                        ""endpoints"": [
                                                            {
                                                                ""tenantId"": ""10244656540440"",
                                                                ""publicURL"": ""https://region-a.geo-1.objects.hpcloudsvc.com/v1/10244656540440"",
                                                                ""region"": ""region-a.geo-1"",
                                                                ""versionId"": ""1.0"",
                                                                ""versionInfo"": ""https://region-a.geo-1.objects.hpcloudsvc.com/v1.0/"",
                                                                ""versionList"": ""https://region-a.geo-1.objects.hpcloudsvc.com""
                                                            },
                                                            {
                                                                ""tenantId"": ""10244656540440"",
                                                                ""publicURL"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443/v1/10244656540440"",
                                                                ""region"": ""region-b.geo-1"",
                                                                ""versionId"": ""1"",
                                                                ""versionInfo"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443/v1/"",
                                                                ""versionList"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443""
                                                            }
                                                        ]
                                                    }]
                                                }
                                            }";

            var content = TestHelper.CreateStream(payload);

            var restResp = new HttpResponseAbstraction(content, new HttpHeadersAbstraction(), HttpStatusCode.NonAuthoritativeInformation);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            var result = await client.Authenticate();

            Assert.AreEqual(expectedRegion, result.Region);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotAuthenticateWithUnauthorizedResponse()
        {
            var creds = GetValidCredentials();

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Unauthorized);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            await client.Authenticate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotAuthenticateWithBadRequestResponse()
        {
            var creds = GetValidCredentials();

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.BadRequest);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            await client.Authenticate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotAuthenticateWithInternalServerErrorResponse()
        {
            var creds = GetValidCredentials();

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            await client.Authenticate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotAuthenticateWithForbiddenResponse()
        {
            var creds = GetValidCredentials();

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.Forbidden);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            await client.Authenticate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotAuthenticateWithBadMethodResponse()
        {
            var creds = GetValidCredentials();

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.MethodNotAllowed);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            await client.Authenticate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotAuthenticateWithOverLimitResponse()
        {
            var creds = GetValidCredentials();

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.RequestEntityTooLarge);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            await client.Authenticate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotAuthenticateWithServiceUnavailableResponse()
        {
            var creds = GetValidCredentials();

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.ServiceUnavailable);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            await client.Authenticate();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotAuthenticateWithNotFoundResponse()
        {
            var creds = GetValidCredentials();

            var restResp = new HttpResponseAbstraction(new MemoryStream(), new HttpHeadersAbstraction(), HttpStatusCode.NotFound);
            this.RestClient.Response = restResp;

            var client = new IdentityServicePocoClient(creds, "Swift", CancellationToken.None, this.ServiceLocator);
            await client.Authenticate();
        }
    }
}
