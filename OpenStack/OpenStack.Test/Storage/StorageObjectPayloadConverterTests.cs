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
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OpenStack.Common.Http;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class StorageObjectPayloadConverterTests
    {
        [TestMethod]
        public void CanParseValidJsonPayloadWithMultipleObjects()
        {
            var validMultipleObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            },
                                            {
                                                ""hash"": ""97cdd4bb45c3d5d652c0079901fb4eec"",
                                                ""last_modified"": ""2014-03-05T01:10:22.786140"",
                                                ""bytes"": 2147483649,
                                                ""name"": ""LargeFile.bin"",
                                                ""content_type"": ""application/octet-stream""
                                            }
                                           ]";

            var converter = new StorageObjectPayloadConverter();
            var objects = converter.Convert("TestContainer", validMultipleObjectJson).ToList();

            Assert.AreEqual(2,objects.Count());
            var obj1 =
                objects.First(o => string.Equals(o.Name, "BLAH", StringComparison.InvariantCultureIgnoreCase));
            var obj2 =
                objects.First(o => string.Equals(o.Name, "LargeFile.bin", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(obj1);
            Assert.IsNotNull(obj2);

            Assert.AreEqual(0,obj1.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj1.ETag);
            Assert.AreEqual("application/octet-stream", obj1.ContentType);
            Assert.AreEqual(DateTime.Parse("2014-03-07T21:31:31.588170"), obj1.LastModified);
            Assert.AreEqual("BLAH", obj1.Name);
            Assert.AreEqual("TestContainer", obj1.ContainerName);

            Assert.AreEqual(2147483649, obj2.Length);
            Assert.AreEqual("97cdd4bb45c3d5d652c0079901fb4eec", obj2.ETag);
            Assert.AreEqual("application/octet-stream", obj2.ContentType);
            Assert.AreEqual(DateTime.Parse("2014-03-05T01:10:22.786140"), obj2.LastModified);
            Assert.AreEqual("LargeFile.bin", obj2.Name);
            Assert.AreEqual("TestContainer", obj2.ContainerName);
        }

        [TestMethod]
        public void CanParseValidJsonPayloadWithSingleObject()
        {
            var validSingleObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageObjectPayloadConverter();
            var objects = converter.Convert("TestContainer", validSingleObjectJson).ToList();

            Assert.AreEqual(1, objects.Count());
            var obj1 =
                objects.First(o => string.Equals(o.Name, "BLAH", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(obj1);

            Assert.AreEqual(0, obj1.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj1.ETag);
            Assert.AreEqual("application/octet-stream", obj1.ContentType);
            Assert.AreEqual(DateTime.Parse("2014-03-07T21:31:31.588170"), obj1.LastModified);
            Assert.AreEqual("BLAH", obj1.Name);
            Assert.AreEqual("TestContainer", obj1.ContainerName);
        }

        [TestMethod]
        public void CanParseValidEmptyJsonArrayPayload()
        {
            var emptyJsonArray = @"[]";

            var converter = new StorageObjectPayloadConverter();
            var objects = converter.Convert("TestContainer", emptyJsonArray).ToList();

            Assert.AreEqual(0, objects.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyPayload()
        {
            var payload = string.Empty;

            var converter = new StorageObjectPayloadConverter();
            var objects = converter.Convert("TestContainer", payload).ToList();

            Assert.AreEqual(0, objects.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullPayload()
        {
            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidJsonPayload()
        {
            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", "[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidPayload()
        {
            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", "NOT JSON");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithMissingBytesProperty()
        {
            string InvalidJsonWithoutBytes = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }
                                           ]";

            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", InvalidJsonWithoutBytes);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithMissingHashProperty()
        {
            string InvalidJsonWithoutHash = @"[
                                            {
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", InvalidJsonWithoutHash);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithMissingModifiedProperty()
        {
            string InvalidJsonWithoutLastModified = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", InvalidJsonWithoutLastModified);
        }

        [TestMethod]
        public void CannotParseJsonPayloadWithMissingNameProperty()
        {
            string InvalidJsonWithoutName = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageObjectPayloadConverter();
            
            try
            {
                converter.Convert("TestContainer", InvalidJsonWithoutName);
                Assert.Fail("Parsing did not fail as expected.");
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Storage Object payload could not be parsed."));
            }
        }

        [TestMethod]
        public void ParseExceptionIncludesNameWhenPossible()
        {
            string InvalidJsonWithoutHash = @"[
                                            {
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageObjectPayloadConverter();
            try
            {
                converter.Convert("TestContainer", InvalidJsonWithoutHash);
                Assert.Fail("Parsing did not fail as expected.");
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Storage Object 'BLAH'"));
            }
            
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithMissingContentTypeProperty()
        {
            string InvalidJsonWithoutContentType = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH""
                                            }]";

            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", InvalidJsonWithoutContentType);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithBadModifiedDate()
        {
            string InvalidJsonWithBadDateType = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""This is not a date"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", InvalidJsonWithBadDateType);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithBadBytesValue()
        {
            string InvalidJsonWithBadBytesValue = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": ""This is not a number"",
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageObjectPayloadConverter();
            converter.Convert("TestContainer", InvalidJsonWithBadBytesValue);
        }

        [TestMethod]
        public void CanParseObjectFromHeaders()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            var obj = converter.Convert(containerName, objectName, headers);

            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(StorageObject));
            Assert.AreEqual(1234, obj.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj.ETag);
            Assert.AreEqual("application/octet-stream", obj.ContentType);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), obj.LastModified);
            Assert.AreEqual(objectName, obj.Name);
            Assert.AreEqual(containerName, obj.ContainerName);
        }

        [TestMethod]
        public void CanParseObjectFromHeadersWithMetadata()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"}
            };

            var converter = new StorageObjectPayloadConverter();
            var obj = converter.Convert(containerName, objectName, headers);

            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(StorageObject));
            Assert.AreEqual(1234, obj.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj.ETag);
            Assert.AreEqual("application/octet-stream", obj.ContentType);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), obj.LastModified);
            Assert.AreEqual(objectName, obj.Name);
            Assert.AreEqual(containerName, obj.ContainerName);
            Assert.AreEqual(1, obj.Metadata.Count());
            Assert.IsTrue(obj.Metadata.ContainsKey("Test1"));
            Assert.AreEqual("Test1", obj.Metadata["Test1"]);
        }

        [TestMethod]
        public void CanParseStaticManifestFromHeadersWithMetadata()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Static-Large-Object","True"}
            };

            var converter = new StorageObjectPayloadConverter();
            var obj = converter.Convert(containerName, objectName, headers);

            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(StaticLargeObjectManifest));
            Assert.AreEqual(1234, obj.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj.ETag);
            Assert.AreEqual("application/octet-stream", obj.ContentType);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), obj.LastModified);
            Assert.AreEqual(objectName, obj.Name);
            Assert.AreEqual(containerName, obj.ContainerName);
            Assert.AreEqual(1, obj.Metadata.Count());
            Assert.IsTrue(obj.Metadata.ContainsKey("Test1"));
            Assert.AreEqual("Test1", obj.Metadata["Test1"]);
        }

        [TestMethod]
        public void CanParseObjectFromHeadersWithStaticManifestFlagSetToNonTrue()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Static-Large-Object","False"}
            };

            var converter = new StorageObjectPayloadConverter();
            var obj = converter.Convert(containerName, objectName, headers);

            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(StorageObject));
            Assert.AreEqual(1234, obj.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj.ETag);
            Assert.AreEqual("application/octet-stream", obj.ContentType);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), obj.LastModified);
            Assert.AreEqual(objectName, obj.Name);
            Assert.AreEqual(containerName, obj.ContainerName);
            Assert.AreEqual(1, obj.Metadata.Count());
            Assert.IsTrue(obj.Metadata.ContainsKey("Test1"));
            Assert.AreEqual("Test1", obj.Metadata["Test1"]);
        }

        [TestMethod]
        public void CanParseObjectFromHeadersWithStaticManifestFlagSetToNull()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Static-Large-Object", (string)null}
            };

            var converter = new StorageObjectPayloadConverter();
            var obj = converter.Convert(containerName, objectName, headers);

            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(StorageObject));
            Assert.AreEqual(1234, obj.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj.ETag);
            Assert.AreEqual("application/octet-stream", obj.ContentType);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), obj.LastModified);
            Assert.AreEqual(objectName, obj.Name);
            Assert.AreEqual(containerName, obj.ContainerName);
            Assert.AreEqual(1, obj.Metadata.Count());
            Assert.IsTrue(obj.Metadata.ContainsKey("Test1"));
            Assert.AreEqual("Test1", obj.Metadata["Test1"]);
        }

        [TestMethod]
        public void CanParseObjectFromHeadersWithDynamicFlagSetToNull()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Object-Manifest", (string)null}
            };

            var converter = new StorageObjectPayloadConverter();
            var obj = converter.Convert(containerName, objectName, headers);

            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(StorageObject));
            Assert.AreEqual(1234, obj.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj.ETag);
            Assert.AreEqual("application/octet-stream", obj.ContentType);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), obj.LastModified);
            Assert.AreEqual(objectName, obj.Name);
            Assert.AreEqual(containerName, obj.ContainerName);
            Assert.AreEqual(1, obj.Metadata.Count());
            Assert.IsTrue(obj.Metadata.ContainsKey("Test1"));
            Assert.AreEqual("Test1", obj.Metadata["Test1"]);
        }

        [TestMethod]
        public void CanParseObjectFromHeadersWithDynamicFlagSetToEmpty()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Object-Manifest", string.Empty}
            };

            var converter = new StorageObjectPayloadConverter();
            var obj = converter.Convert(containerName, objectName, headers);

            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(StorageObject));
            Assert.AreEqual(1234, obj.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj.ETag);
            Assert.AreEqual("application/octet-stream", obj.ContentType);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), obj.LastModified);
            Assert.AreEqual(objectName, obj.Name);
            Assert.AreEqual(containerName, obj.ContainerName);
            Assert.AreEqual(1, obj.Metadata.Count());
            Assert.IsTrue(obj.Metadata.ContainsKey("Test1"));
            Assert.AreEqual("Test1", obj.Metadata["Test1"]);
        }

        [TestMethod]
        public void CanParseDynamicManifestFromHeadersWithMetadata()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"ETag", "d41d8cd98f00b204e9800998ecf8427e"},
                {"X-Object-Meta-Test1","Test1"},
                {"X-Object-Manifest","a/b"}
            };

            var converter = new StorageObjectPayloadConverter();
            var obj = converter.Convert(containerName, objectName, headers);

            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(DynamicLargeObjectManifest));
            Assert.AreEqual(1234, obj.Length);
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", obj.ETag);
            Assert.AreEqual("application/octet-stream", obj.ContentType);
            Assert.AreEqual(DateTime.Parse("Wed, 12 Mar 2014 23:42:23 GMT"), obj.LastModified);
            Assert.AreEqual(objectName, obj.Name);
            Assert.AreEqual(containerName, obj.ContainerName);
            Assert.AreEqual(1, obj.Metadata.Count());
            Assert.IsTrue(obj.Metadata.ContainsKey("Test1"));
            Assert.AreEqual("Test1", obj.Metadata["Test1"]);

            var manifest = obj as DynamicLargeObjectManifest;
            Assert.AreEqual("a/b", manifest.SegmentsPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseWithANullObjectName()
        {
            var containerName = "TestContainer";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, null, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotParseWithAnEmptyConatinerName()
        {
            var containerName = string.Empty;
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseWithNullHeaders()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotParseWithAnEmptyObjectName()
        {
            var containerName = "TestContainer";
            var objectName =string.Empty;

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseWithNullConatinerName()
        {
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(null, objectName, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseWithMissingContentLengthHeader()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseWithMissingETagHeader()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseWithMissingModifiedHeader()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseWithMissingContentTypeHeader()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseWithBadModifiedDateHeader()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "1234"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "This is not a date"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, headers);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseWithBadBytesHeader()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var headers = new HttpHeadersAbstraction()
            {
                {"Content-Length", "This is not a number"},
                {"Content-Type", "application/octet-stream"},
                {"Last-Modified", "Wed, 12 Mar 2014 23:42:23 GMT"},
                {"Etag", "d41d8cd98f00b204e9800998ecf8427e"}
            };

            var converter = new StorageObjectPayloadConverter();
            converter.Convert(containerName, objectName, headers);
        }

        [TestMethod]
        public void CanConvertSingleStorageObjectToJson()
        {
            var obj = new StorageObject("a/b/c", "TestContainer", DateTime.UtcNow, "12345", 54321, string.Empty,
                new Dictionary<string, string>());

            var converter = new StorageObjectPayloadConverter();
            var payload = converter.Convert(new List<StorageObject>() { obj});

            var result = JArray.Parse(payload);
            Assert.AreEqual(1,result.Count);

            var item = result[0];
            Assert.AreEqual("TestContainer/a/b/c", item["path"]);
            Assert.AreEqual(54321, item["size_bytes"]);
            Assert.AreEqual("12345", item["etag"]);
        }

        [TestMethod]
        public void CanConvertMultipleStorageObjectToJson()
        {
            var obj = new StorageObject("a/b/c", "TestContainer", DateTime.UtcNow, "12345", 54321, string.Empty,
                new Dictionary<string, string>());

            var obj2 = new StorageObject("a/b/d", "TestContainer", DateTime.UtcNow, "00000", 11111, string.Empty,
                new Dictionary<string, string>());

            var converter = new StorageObjectPayloadConverter();
            var payload = converter.Convert(new List<StorageObject>() { obj, obj2 });

            var result = JArray.Parse(payload);
            Assert.AreEqual(2, result.Count);

            var item = result[0];
            Assert.AreEqual("TestContainer/a/b/c", item["path"]);
            Assert.AreEqual(54321, item["size_bytes"]);
            Assert.AreEqual("12345", item["etag"]);

            var item2 = result[1];
            Assert.AreEqual("TestContainer/a/b/d", item2["path"]);
            Assert.AreEqual(11111, item2["size_bytes"]);
            Assert.AreEqual("00000", item2["etag"]);
        }

        [TestMethod]
        public void CanConvertEmptyStorageObjectsToJson()
        {
            var converter = new StorageObjectPayloadConverter();
            var payload = converter.Convert(new List<StorageObject>());
            var result = JArray.Parse(payload);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertNullStorageObjectsToJson()
        {
            var converter = new StorageObjectPayloadConverter();
            converter.Convert(null);
        }
    }
}
