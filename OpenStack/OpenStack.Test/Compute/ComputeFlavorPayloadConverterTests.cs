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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    [TestClass]
    public class ComputeFlavorPayloadConverterTests
    {
        internal string CreateFlavorJsonFixtrue(string name, string id, string ram, string disk, string vcpus,
            string permaUri, string publicUri)
        {
            var ComputeFlavorJsonResponseFixture = @"{{
                                    ""flavor"" : {{
                                        ""name"": ""{0}"",
                                        ""links"": [
                                            {{
                                                ""href"": ""{1}"",
                                                ""rel"": ""self""
                                            }},
                                            {{
                                                ""href"": ""{2}"",
                                                ""rel"": ""bookmark""
                                            }}
                                        ],
                                        ""ram"": {3},
                                        ""vcpus"": {4},
                                        ""disk"": {5},
                                        ""id"": ""{6}""
                                    }}
                                }}";

            return string.Format(ComputeFlavorJsonResponseFixture, name, publicUri, permaUri, ram, vcpus, disk, id);
        }

        internal string CreateFlavorSummaryJsonFixtrue(string name, string id, string permaUri, string publicUri)
        {
            var computeFlavorSummaryJsonResponseFixture = @"{{
                                                        ""id"": ""{0}"",
                                                        ""links"": [
                                                            {{
                                                                ""href"": ""{1}"",
                                                                ""rel"": ""self""
                                                            }},
                                                            {{
                                                                ""href"": ""{2}"",
                                                                ""rel"": ""bookmark""
                                                            }}
                                                        ],
                                                        ""name"": ""{3}""
                                                    }}";

            return string.Format(computeFlavorSummaryJsonResponseFixture, id, publicUri, permaUri, name);
        }

        [TestMethod]
        public void CanConvertJsonPayloadToFlavor()
        {
            var flavorName = "m1.tiny";
            var flavorId = "1";
            var flavorPublicUri = "http://www.server.com/v2/flavors/1";
            var flavorPermUri = "http://www.server.com/flavors/1";
            var flavorRam = "512";
            var flavorDisk = "10";
            var flavorVcpus = "2";

            var computeFlavorJsonResponseFixture = CreateFlavorJsonFixtrue(flavorName, flavorId, flavorRam, flavorDisk,
                flavorVcpus, flavorPermUri, flavorPublicUri);

            var converter = new ComputeFlavorPayloadConverter();
            var flavor = converter.ConvertFlavor(computeFlavorJsonResponseFixture);
            Assert.IsNotNull(flavor);
            Assert.AreEqual(flavorName, flavor.Name);
            Assert.AreEqual(flavorId, flavor.Id);
            Assert.AreEqual(flavorRam, flavor.Ram);
            Assert.AreEqual(flavorVcpus, flavor.Vcpus);
            Assert.AreEqual(flavorDisk, flavor.Disk);
            Assert.AreEqual(new Uri(flavorPermUri), flavor.PermanentUri);
            Assert.AreEqual(new Uri(flavorPublicUri), flavor.PublicUri);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingRamToFlavor()
        {
            var missingFixture = @"{
                                    ""flavor"": {
                                        ""name"": ""m1.tiny"",
                                        ""id"": ""1"",
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/flavors/1"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/flavors/1"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""vcpus"": 2,
                                        ""disk"": 10
                                    }
                                }";

            var converter = new ComputeFlavorPayloadConverter();
            var flavor = converter.ConvertFlavor(missingFixture);
            Assert.IsNotNull(flavor);
            Assert.AreEqual("m1.tiny", flavor.Name);
            Assert.AreEqual("1", flavor.Id);
            Assert.AreEqual("2", flavor.Vcpus);
            Assert.AreEqual("10", flavor.Disk);
            Assert.AreEqual(new Uri("http://someuri.com/flavors/1"), flavor.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/flavors/1"), flavor.PublicUri);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingVcpusToFlavor()
        {
            var missingFixture = @"{
                                    ""flavor"": {
                                        ""name"": ""m1.tiny"",
                                        ""id"": ""1"",
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/flavors/1"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/flavors/1"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""ram"": 512,
                                        ""disk"": 10
                                    }
                                }";

            var converter = new ComputeFlavorPayloadConverter();
            var flavor = converter.ConvertFlavor(missingFixture);
            Assert.IsNotNull(flavor);
            Assert.AreEqual("m1.tiny", flavor.Name);
            Assert.AreEqual("1", flavor.Id);
            Assert.AreEqual("512", flavor.Ram);
            Assert.AreEqual("10", flavor.Disk);
            Assert.AreEqual(new Uri("http://someuri.com/flavors/1"), flavor.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/flavors/1"), flavor.PublicUri);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingDiskToFlavor()
        {
            var missingFixture = @"{
                                    ""flavor"": {
                                        ""name"": ""m1.tiny"",
                                        ""id"": ""1"",
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/flavors/1"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/flavors/1"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""ram"": 512,
                                        ""vcpus"": 2
                                    }
                                }";

            var converter = new ComputeFlavorPayloadConverter();
            var flavor = converter.ConvertFlavor(missingFixture);
            Assert.IsNotNull(flavor);
            Assert.AreEqual("m1.tiny", flavor.Name);
            Assert.AreEqual("1", flavor.Id);
            Assert.AreEqual("512", flavor.Ram);
            Assert.AreEqual("2", flavor.Vcpus);
            Assert.AreEqual(new Uri("http://someuri.com/flavors/1"), flavor.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/flavors/1"), flavor.PublicUri);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingIdToFlavor()
        {
            var missingIdFixture = @"{
                                    ""flavor"" : {
                                        ""name"": ""m1.tiny"",
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/flavors/1"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/flavors/1"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""ram"": 512,
                                        ""vcpus"": 2,
                                        ""disk"": 10
                                    }
                                }";

            var converter = new ComputeFlavorPayloadConverter();
            converter.ConvertFlavor(missingIdFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingNameToFlavor()
        {
            var missingTokenFixture = @"{
                                    ""flavor"": {
                                        ""name"" : ""m1.tiny"",
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/flavors/1"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/flavors/1"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""ram"": 512,
                                        ""vcpus"": 2,
                                        ""disk"": 10
                                    }
                                }";

            var converter = new ComputeFlavorPayloadConverter();
            converter.ConvertFlavor(missingTokenFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadEmptyObjectToFlavor()
        {
            var emptyObjectFixture = @"{ }";

            var converter = new ComputeFlavorPayloadConverter();
            converter.ConvertFlavor(emptyObjectFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidJsonToFlavor()
        {
            var badJsonFixture = @"{ NOT JSON";

            var converter = new ComputeFlavorPayloadConverter();
            converter.ConvertFlavor(badJsonFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectJsonToFlavor()
        {
            var nonObjectJson = @"[]";

            var converter = new ComputeFlavorPayloadConverter();
            converter.ConvertFlavor(nonObjectJson);
        }

        [TestMethod]
        public void CanParseValidFlavorsJsonPayloadWithMultipleFlavors()
        {
            var validMultipleFlavorsJsonFixture = @"{{ ""flavors"": [ {0} ] }}";
            var firstFlavor = CreateFlavorSummaryJsonFixtrue("m1.tiny", "1", "http://server.com/flavors/1",
                "http://server.com/v2/flavors/1");
            var secondFlavor = CreateFlavorSummaryJsonFixtrue("m1.small", "2", "http://server.com/flavors/2",
               "http://server.com/v2/flavors/2");

            var validMultipleFlavorsJson = string.Format(validMultipleFlavorsJsonFixture,
                string.Join(",", new List<string>() {firstFlavor, secondFlavor}));

            var converter = new ComputeFlavorPayloadConverter();
            var flavors = converter.ConvertFlavors(validMultipleFlavorsJson).ToList();

            Assert.AreEqual(2, flavors.Count());
            var flv1 =
                flavors.First(o => string.Equals(o.Name, "m1.tiny", StringComparison.InvariantCultureIgnoreCase));
            var flv2 =
                flavors.First(o => string.Equals(o.Name, "m1.small", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(flv1);
            Assert.IsNotNull(flv2);

            Assert.AreEqual("1", flv1.Id);
            Assert.AreEqual(new Uri("http://server.com/flavors/1"), flv1.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/flavors/1"), flv1.PublicUri);

            Assert.AreEqual("2", flv2.Id);
            Assert.AreEqual(new Uri("http://server.com/flavors/2"), flv2.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/flavors/2"), flv2.PublicUri);
        }

        [TestMethod]
        public void CanConvertValidFlavorsJsonPayloadWithSingleFlavor()
        {
            var validFlavorsJsonFixture = @"{{ ""flavors"": [ {0} ] }}";
            var firstFlavor = CreateFlavorSummaryJsonFixtrue("m1.tiny", "1", "http://server.com/flavors/1",
                "http://server.com/v2/flavors/1");
            var validMultipleFlavorsJson = string.Format(validFlavorsJsonFixture, firstFlavor);

            var converter = new ComputeFlavorPayloadConverter();
            var flavors = converter.ConvertFlavors(validMultipleFlavorsJson).ToList();

            Assert.AreEqual(1, flavors.Count());
            var flv1 =
                flavors.First(o => string.Equals(o.Name, "m1.tiny", StringComparison.InvariantCultureIgnoreCase));
           
            Assert.IsNotNull(flv1);

            Assert.AreEqual("1", flv1.Id);
            Assert.AreEqual(new Uri("http://server.com/flavors/1"), flv1.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/flavors/1"), flv1.PublicUri);
        }

        [TestMethod]
        public void CanParseValidFlavorsPayloadWithEmptyJsonArray()
        {
            var emptyJsonArray = @"{ ""flavors"": [ ] }";

            var converter = new ComputeFlavorPayloadConverter();
            var containers = converter.ConvertFlavors(emptyJsonArray).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyFlavorsPayload()
        {
            var payload = string.Empty;

            var converter = new ComputeFlavorPayloadConverter();
            var containers = converter.ConvertFlavors(payload).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullFlavorsPayload()
        {
            var converter = new ComputeFlavorPayloadConverter();
            converter.ConvertFlavors(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidFlavorsJsonPayload()
        {
            var converter = new ComputeFlavorPayloadConverter();
            converter.ConvertFlavors("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidFlavorsPayload()
        {
            var converter = new ComputeFlavorPayloadConverter();
            converter.ConvertFlavors("NOT JSON");
        }
    }
}
