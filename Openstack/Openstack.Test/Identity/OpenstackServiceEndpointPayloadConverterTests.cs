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
using Openstack.Identity;

namespace Openstack.Test.Identity
{
    [TestClass]
    public class OpenstackServiceEndpointPayloadConverterTests
    {
        [TestMethod]
        public void CanConvertJsonPayload()
        {
            var expectedPublicUri = "https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440";
            var expectedRegion = "region-a.geo-1";
            var expectedVersion = "1.0";
            var expectedVersionList = "https://region-a.geo-1.block.hpcloudsvc.com/";
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

            var converter = new OpenstackServiceEndpointPayloadConverter();
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
            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
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

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
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

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotConvertJsonPayloadWithMissingVersion()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotConvertJsonPayloadWithMissingVersionInfoUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionList"": ""https://region-a.geo-1.block.hpcloudsvc.com""
                    }";

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotConvertJsonPayloadWithMissingVersionListUri()
        {
            var endpointPayload = @" {
                        ""tenantId"": ""10244656540440"",
                        ""publicURL"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1/10244656540440"",
                        ""publicURL2"": """",
                        ""region"": ""region-a.geo-1"",
                        ""versionId"": ""1.0"",
                        ""versionInfo"": ""https://region-a.geo-1.block.hpcloudsvc.com/v1""
                    }";

            var converter = new OpenstackServiceEndpointPayloadConverter();
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

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotConvertJsonPayloadWithBadVersionInfoUri()
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

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotConvertJsonPayloadWithBadVersionListcUri()
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

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotConvertInvalidJsonPayload()
        {
            var endpointPayload = @" { NOT JSON";

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotConvertNonObjectJsonPayload()
        {
            var endpointPayload = @" [] ";

            var converter = new OpenstackServiceEndpointPayloadConverter();
            converter.Convert(endpointPayload);
        }
    }
}
