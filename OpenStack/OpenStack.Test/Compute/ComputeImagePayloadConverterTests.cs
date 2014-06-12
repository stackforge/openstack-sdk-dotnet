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
    public class ComputeImagePayloadConverterTests
    {
        internal string CreateImageJsonFixtrue(string name, string id, string permaUri, string publicUri, string status, string minDisk, string minRam, string progress, string created, string updated)
        {
            var ComputeFlavorJsonResponseFixture = @"{{
                                    ""image"" : {{
                                        ""name"": ""{0}"",
                                        ""status"": ""{1}"",
                                        ""updated"": ""{2}"",
                                        ""created"": ""{3}"",
                                        ""minDisk"": {4},
                                        ""progress"": {5},
                                        ""minRam"": {6},
                                        ""links"": [
                                            {{
                                                ""href"": ""{7}"",
                                                ""rel"": ""self""
                                            }},
                                            {{
                                                ""href"": ""{8}"",
                                                ""rel"": ""bookmark""
                                            }}
                                        ],
                                        ""metadata"": {{
                                            ""kernel_id"": ""nokernel"",
                                            ""ramdisk_id"": ""98765""
                                        }},
                                        ""id"": ""{9}""
                                    }}
                                }}";

            return string.Format(ComputeFlavorJsonResponseFixture, name, status, updated, created, minDisk, progress, minRam, publicUri, permaUri, id);
        }

        internal string CreateImageSummaryJsonFixtrue(string name, string id, string permaUri, string publicUri)
        {
            var computeImageSummaryJsonResponseFixture = @"{{
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

            return string.Format(computeImageSummaryJsonResponseFixture, id, publicUri, permaUri, name);
        }

        [TestMethod]
        public void CanConvertJsonPayloadToImage()
        {
            var imageName = "myimage";
            var imageId = "12345";
            var imagePublicUri = "http://www.server.com/v2/images/12345";
            var imagePermUri = "http://www.server.com/images/12345";
            var minRam = 512;
            var minDisk = 10;
            var progress = 100;
            var status = "ACTIVE";

            var lastUpdate = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10));
            var createdDate = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(100));
            var created = createdDate.ToLongTimeString();
            var updated = lastUpdate.ToLongTimeString();

            var computeFlavorJsonResponseFixture = CreateImageJsonFixtrue(imageName, imageId, imagePermUri, imagePublicUri,
                status, minDisk.ToString(), minRam.ToString(), progress.ToString(), created,updated);

            var converter = new ComputeImagePayloadConverter();
            var image = converter.ConvertImage(computeFlavorJsonResponseFixture);
            Assert.IsNotNull(image);
            Assert.AreEqual(imageName, image.Name);
            Assert.AreEqual(imageId, image.Id);
            Assert.AreEqual(minRam, image.MinimumRamSize);
            Assert.AreEqual(progress, image.UploadProgress);
            Assert.AreEqual(minDisk, image.MinimumDiskSize);
            Assert.AreEqual(status, image.Status);
            Assert.AreEqual(lastUpdate.ToLongTimeString(), image.LastUpdated.ToLongTimeString());
            Assert.AreEqual(createdDate.ToLongTimeString(), image.CreateDate.ToLongTimeString());
            Assert.AreEqual(new Uri(imagePermUri), image.PermanentUri);
            Assert.AreEqual(new Uri(imagePublicUri), image.PublicUri);
            Assert.AreEqual(2, image.Metadata.Count);
            Assert.AreEqual("nokernel",image.Metadata["kernel_id"]);
            Assert.AreEqual("98765",image.Metadata["ramdisk_id"]);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingMinRamToImage()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""progress"": 100,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            var image = converter.ConvertImage(missingFixture);
            Assert.IsNotNull(image);
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual("ACTIVE", image.Status);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(0, image.MinimumRamSize);
            Assert.AreEqual(10, image.MinimumDiskSize);
            Assert.AreEqual(100, image.UploadProgress);
            Assert.AreEqual(created.ToLongTimeString(), image.CreateDate.ToLongTimeString());
            Assert.AreEqual(updated.ToLongTimeString(), image.LastUpdated.ToLongTimeString());
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
        }

        public void CanConvertJsonPayloadMissingMetadataToImage()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""minRam"": 512,
                                        ""progress"": 100,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            var image = converter.ConvertImage(missingFixture);
            Assert.IsNotNull(image);
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual("ACTIVE", image.Status);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(512, image.MinimumRamSize);
            Assert.AreEqual(10, image.MinimumDiskSize);
            Assert.AreEqual(100, image.UploadProgress);
            Assert.AreEqual(created.ToLongTimeString(), image.CreateDate.ToLongTimeString());
            Assert.AreEqual(updated.ToLongTimeString(), image.LastUpdated.ToLongTimeString());
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
            Assert.AreEqual(0, image.Metadata.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadWithBadMetadataToImage()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""minRam"": 512,
                                        ""progress"": 100,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""metadata"": {
                                            ""kernel_id"": { ""NotExpectyed"" : ""SomeBadValue"" },
                                            ""ramdisk_id"": ""98765""
                                        },
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImage(missingFixture);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingMinDiskToImage()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""progress"": 100,
                                        ""minRam"": 512,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            var image = converter.ConvertImage(missingFixture);
            Assert.IsNotNull(image);
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual("ACTIVE", image.Status);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(512, image.MinimumRamSize);
            Assert.AreEqual(0, image.MinimumDiskSize);
            Assert.AreEqual(100, image.UploadProgress);
            Assert.AreEqual(created.ToLongTimeString(), image.CreateDate.ToLongTimeString());
            Assert.AreEqual(updated.ToLongTimeString(), image.LastUpdated.ToLongTimeString());
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingStatusToImage()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""progress"": 100,
                                        ""minRam"": 512,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            var image = converter.ConvertImage(missingFixture);
            Assert.IsNotNull(image);
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual(string.Empty, image.Status);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(512, image.MinimumRamSize);
            Assert.AreEqual(10, image.MinimumDiskSize);
            Assert.AreEqual(100, image.UploadProgress);
            Assert.AreEqual(created.ToLongTimeString(), image.CreateDate.ToLongTimeString());
            Assert.AreEqual(updated.ToLongTimeString(), image.LastUpdated.ToLongTimeString());
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingUpdateDateToImage()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.MinValue;
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""progress"": 100,
                                        ""minRam"": 512,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            var image = converter.ConvertImage(missingFixture);
            Assert.IsNotNull(image);
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual("ACTIVE", image.Status);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(512, image.MinimumRamSize);
            Assert.AreEqual(10, image.MinimumDiskSize);
            Assert.AreEqual(100, image.UploadProgress);
            Assert.AreEqual(created.ToLongTimeString(), image.CreateDate.ToLongTimeString());
            Assert.AreEqual(updated.ToLongTimeString(), image.LastUpdated.ToLongTimeString());
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissinCreatedDateToImage()
        {
            var created = DateTime.MinValue;
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""progress"": 100,
                                        ""minRam"": 512,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            var image = converter.ConvertImage(missingFixture);
            Assert.IsNotNull(image);
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual("ACTIVE", image.Status);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(512, image.MinimumRamSize);
            Assert.AreEqual(10, image.MinimumDiskSize);
            Assert.AreEqual(100, image.UploadProgress);
            Assert.AreEqual(created.ToLongTimeString(), image.CreateDate.ToLongTimeString());
            Assert.AreEqual(updated.ToLongTimeString(), image.LastUpdated.ToLongTimeString());
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingProgressToImage()
        {
            var created = DateTime.Parse("2014-05-30T16:56:32Z").ToUniversalTime();
            var updated = DateTime.Parse("2014-06-30T16:56:32Z").ToUniversalTime();
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""minRam"": 512,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            var image = converter.ConvertImage(missingFixture);
            Assert.IsNotNull(image);
            Assert.AreEqual("image1", image.Name);
            Assert.AreEqual("ACTIVE", image.Status);
            Assert.AreEqual("12345", image.Id);
            Assert.AreEqual(512, image.MinimumRamSize);
            Assert.AreEqual(10, image.MinimumDiskSize);
            Assert.AreEqual(0, image.UploadProgress);
            Assert.AreEqual(created, image.CreateDate);
            Assert.AreEqual(updated, image.LastUpdated);
            Assert.AreEqual(new Uri("http://someuri.com/images/12345"), image.PermanentUri);
            Assert.AreEqual(new Uri("http://someuri.com/v2/images/12345"), image.PublicUri);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingIdToImage()
        {
            var missingFixture = @"{
                                    ""image"" : {
                                        ""name"": ""image1"",
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""progress"": 100,
                                        ""minRam"": 512,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ]
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImage(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingNameToImage()
        {
            var missingFixture = @"{
                                    ""image"" : {
                                        ""status"": ""ACTIVE"",
                                        ""updated"": ""2014-06-30T16:56:32Z"",
                                        ""created"": ""2014-05-30T16:56:32Z"",
                                        ""minDisk"": 10,
                                        ""progress"": 100,
                                        ""minRam"": 512,
                                        ""links"": [
                                            {
                                                ""href"": ""http://someuri.com/v2/images/12345"",
                                                ""rel"": ""self""
                                            },
                                            {
                                                ""href"": ""http://someuri.com/images/12345"",
                                                ""rel"": ""bookmark""
                                            }
                                        ],
                                        ""id"": ""12345""
                                    }
                                }";

            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImage(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadEmptyObjectToImage()
        {
            var emptyObjectFixture = @"{ }";

            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImage(emptyObjectFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidJsonToImage()
        {
            var badJsonFixture = @"{ NOT JSON";

            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImage(badJsonFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectJsonToImage()
        {
            var nonObjectJson = @"[]";

            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImage(nonObjectJson);
        }

        [TestMethod]
        public void CanParseValidFlavorsJsonPayloadWithMultipleFlavors()
        {
            var validMultipleImagesJsonFixture = @"{{ ""images"": [ {0} ] }}";
            var firstImage = CreateImageSummaryJsonFixtrue("image1", "12345", "http://server.com/images/12345",
                "http://server.com/v2/images/12345");
            var secondImage = CreateImageSummaryJsonFixtrue("image2", "23456", "http://server.com/images/23456",
               "http://server.com/v2/images/23456");

            var validMultipleImagesJson = string.Format(validMultipleImagesJsonFixture,
                string.Join(",", new List<string>() { firstImage, secondImage }));

            var converter = new ComputeImagePayloadConverter();
            var images = converter.ConvertImages(validMultipleImagesJson).ToList();

            Assert.AreEqual(2, images.Count());
            var img1 =
                images.First(o => string.Equals(o.Name, "image1", StringComparison.InvariantCultureIgnoreCase));
            var img2 =
                images.First(o => string.Equals(o.Name, "image2", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(img1);
            Assert.IsNotNull(img2);

            Assert.AreEqual("12345", img1.Id);
            Assert.AreEqual(new Uri("http://server.com/images/12345"), img1.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/images/12345"), img1.PublicUri);

            Assert.AreEqual("23456", img2.Id);
            Assert.AreEqual(new Uri("http://server.com/images/23456"), img2.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/images/23456"), img2.PublicUri);
        }

        [TestMethod]
        public void CanConvertValidFlavorsJsonPayloadWithSingleFlavor()
        {
            var validImagesJsonFixture = @"{{ ""images"": [ {0} ] }}";
            var firstImage = CreateImageSummaryJsonFixtrue("image1", "12345", "http://server.com/images/12345",
                "http://server.com/v2/images/12345");
            var validImagesJson = string.Format(validImagesJsonFixture, firstImage);

            var converter = new ComputeImagePayloadConverter();
            var images = converter.ConvertImages(validImagesJson).ToList();

            Assert.AreEqual(1, images.Count());
            var img1 =
                images.First(o => string.Equals(o.Name, "image1", StringComparison.InvariantCultureIgnoreCase));
           
            Assert.IsNotNull(img1);

            Assert.AreEqual("12345", img1.Id);
            Assert.AreEqual(new Uri("http://server.com/images/12345"), img1.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/images/12345"), img1.PublicUri);
        }

        [TestMethod]
        public void CanParseValidImagesPayloadWithEmptyJsonArray()
        {
            var emptyJsonArray = @"{ ""images"": [ ] }";

            var converter = new ComputeImagePayloadConverter();
            var containers = converter.ConvertImages(emptyJsonArray).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyImagesPayload()
        {
            var payload = string.Empty;

            var converter = new ComputeImagePayloadConverter();
            var containers = converter.ConvertImages(payload).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullImagesPayload()
        {
            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImages(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidImagesJsonPayload()
        {
            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImages("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidImagesPayload()
        {
            var converter = new ComputeImagePayloadConverter();
            converter.ConvertImages("NOT JSON");
        }
    }
}
