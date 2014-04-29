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
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class OpenStackServiceDefinitionPayloadConverterTests
    {
        [TestMethod]
        public void CanConvertJsonPayload()
        {
            var expectedName = "Object Storage";
            var expectedType = "object-store";

            var serviceDefPayload = @" {
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
                                    }";

            var converter = new OpenStackServiceDefinitionPayloadConverter(new ServiceLocator());
            var service = converter.Convert(serviceDefPayload);

            Assert.IsNotNull(service);
            Assert.AreEqual(expectedName, service.Name);
            Assert.AreEqual(expectedType, service.Type);
            Assert.AreEqual(2, service.Endpoints.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadWithMissingName()
        {
            var serviceDefPayload = @"{
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
                                    }";

            var converter = new OpenStackServiceDefinitionPayloadConverter(new ServiceLocator());
            converter.Convert(serviceDefPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadWithMissingType()
        {
            var serviceDefPayload = @"{
                                        ""name"": ""Object Storage"",
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
                                    }";

            var converter = new OpenStackServiceDefinitionPayloadConverter(new ServiceLocator());
            converter.Convert(serviceDefPayload);
        }

        [TestMethod]
        [ExpectedException(typeof (FormatException))]
        public void CannotConvertJsonPayloadWithMissingEndpoints()
        {
            var serviceDefPayload = @"{
                                        ""name"": ""Object Storage"",
                                        ""type"": ""object-store"",
                                    }";

            var converter = new OpenStackServiceDefinitionPayloadConverter(new ServiceLocator());
            converter.Convert(serviceDefPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadWithEmptyObject()
        {
            var serviceDefPayload = @" { }";

            var converter = new OpenStackServiceDefinitionPayloadConverter(new ServiceLocator());
            converter.Convert(serviceDefPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertWithNullJsonPayload()
        {
            var converter = new OpenStackServiceDefinitionPayloadConverter(new ServiceLocator());
            converter.Convert(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidJsonPayload()
        {
            var serviceDefPayload = @" { NOT JSON";

            var converter = new OpenStackServiceDefinitionPayloadConverter(new ServiceLocator());
            converter.Convert(serviceDefPayload);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectJsonPayload()
        {
            var serviceDefPayload = @"[]";

            var converter = new OpenStackServiceDefinitionPayloadConverter(new ServiceLocator());
            converter.Convert(serviceDefPayload);
        }
    }
}
