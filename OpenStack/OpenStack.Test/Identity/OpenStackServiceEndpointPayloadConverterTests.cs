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
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class OpenStackServiceEndpointPayloadConverterTests
    {
        [TestMethod]
        public void CanConvertJsonPayload()
        {
            var expectedPublicUri = "https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440";
            var expectedRegion = "region-a.geo-1";
            var expectedVersion = "1.0";
            var expectedVersionList = "https://region-a.geo-1.block.hpcloudsvc.com";
            var expectedVersionInfo = "https://region-a.geo-1.block.hpcloudsvc.com/v1";

            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            var endpoint = converter.Convert(endpointPayload);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(expectedPublicUri, endpoint.PublicUri.ToString());
            Assert.AreEqual(expectedRegion, endpoint.Region);
            Assert.AreEqual(expectedVersion, endpoint.Version);
            Assert.AreEqual(expectedVersionInfo, endpoint.VersionInformation.ToString());
            Assert.AreEqual(expectedVersionList, endpoint.VersionList.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertWithNullJsonPayload()
        {
            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadWithMissingPublicUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadWithMissingRegion()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        public void CanConvertJsonPayloadWithMissingVersion()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        public void CanConvertJsonPayloadWithMissingVersionInfoUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        public void CanConvertJsonPayloadWithMissingVersionListUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        public void CanConvertJsonPayloadWithBadPublicUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""httpsBAD://reg&VERYBAD&ion-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        public void CanConvertJsonPayloadWithBadVersionInfoUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""htBADtps://region&BAD&-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        public void CanConvertJsonPayloadWithBadVersionListcUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": ""htBADtps://region-a.ge&BAD&o-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        public void CanConvertJsonPayloadWithEmptyVersionInfoUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": """",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        public void CanConvertJsonPayloadWithEmptyVersionListcUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": """"
                    }";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidJsonPayload()
        {
            var endpointPayload = @" { NOT JSON";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectJsonPayload()
        {
            var endpointPayload = @" [] ";

            var converter = new OpenStackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }
    }
}
