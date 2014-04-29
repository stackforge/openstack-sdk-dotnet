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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class StorageContainerPayloadConverterTests
    {
        [TestMethod]
        public void CanParseValidJsonPayloadWithMultipleObjects()
        {
            var validMultipleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            },
                                            {
                                                  ""count"": 5,
                                                  ""bytes"": 2000,
                                                  ""name"": ""OtherTestContainer""
                                            }
                                           ]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var containers = converter.Convert(validMultipleContainerJson).ToList();

            Assert.AreEqual(2, containers.Count());
            var obj1 =
                containers.First(o => string.Equals(o.Name, "TestContainer", StringComparison.InvariantCultureIgnoreCase));
            var obj2 =
                containers.First(o => string.Equals(o.Name, "OtherTestContainer", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(obj1);
            Assert.IsNotNull(obj2);

            Assert.AreEqual(7, obj1.TotalBytesUsed);
            Assert.AreEqual("TestContainer", obj1.Name);
            Assert.AreEqual(1, obj1.TotalObjectCount);

            Assert.AreEqual(2000, obj2.TotalBytesUsed);
            Assert.AreEqual("OtherTestContainer", obj2.Name);
            Assert.AreEqual(5, obj2.TotalObjectCount);
        }

        [TestMethod]
        public void CanParseValidJsonPayloadWithSingleObject()
        {
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var containers = converter.Convert(validSingleContainerJson).ToList();

            Assert.AreEqual(1, containers.Count());
            var obj1 =
                containers.First(o => string.Equals(o.Name, "TestContainer", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(obj1);

            Assert.AreEqual(7, obj1.TotalBytesUsed);
            Assert.AreEqual("TestContainer", obj1.Name);
            Assert.AreEqual(1, obj1.TotalObjectCount);
        }

        [TestMethod]
        public void CanParseValidEmptyJsonArrayPayload()
        {
            var emptyJsonArray = @"[]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var containers = converter.Convert(emptyJsonArray).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyPayload()
        {
            var payload = string.Empty;

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var containers = converter.Convert(payload).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullPayload()
        {
            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            converter.Convert(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidJsonPayload()
        {
            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            converter.Convert("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidPayload()
        {
            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            converter.Convert("NOT JSON");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithMissingBytesProperty()
        {
            var InvalidJsonWithoutBytes = @"[
                                            {
                                                  ""count"": 1,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            converter.Convert(InvalidJsonWithoutBytes);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithMissingCountProperty()
        {
            string InvalidJsonWithoutCount = @"[
                                            {
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            converter.Convert(InvalidJsonWithoutCount);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithMissingNameProperty()
        {
            string InvalidJsonWithoutName = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            converter.Convert(InvalidJsonWithoutName);
        }

        [TestMethod]
        public void ParseExceptionIncludesNameWhenPossible()
        {
            string InvalidJsonWithoutBytes = @"[
                                            {
                                                  ""count"": 1,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            try
            {
                converter.Convert(InvalidJsonWithoutBytes);
                Assert.Fail("Parsing did not fail as expected.");
            }
            catch (FormatException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Storage Container 'TestContainer'"));
            }

        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithBadBytesValue()
        {
            string InvalidJsonWithBadBytesValue = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": ""NOT A NUMBER"",
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            converter.Convert(InvalidJsonWithBadBytesValue);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseJsonPayloadWithBadCountValue()
        {
            string InvalidJsonWithBadCountValue = @"[
                                            {
                                                  ""count"": ""NOT A NUMBER"",
                                                  ""bytes"": 12345,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            converter.Convert(InvalidJsonWithBadCountValue);
        }

        [TestMethod]
        public void CanParseContainerWithValidJsonPayloadAndHeaders()
        {
            var containerName = "TestContainer";
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "1"}
            };

            var container = converter.Convert(containerName, headers, validObjectJson);
            Assert.IsNotNull(container);
            Assert.AreEqual(containerName,container.Name);
            Assert.AreEqual(12345, container.TotalBytesUsed);
            Assert.AreEqual(1, container.TotalObjectCount);
            Assert.AreEqual(1, container.Objects.ToList().Count());
        }

        [TestMethod]
        public void CanParseContainerWithValidJsonPayloadWithNestedFoldersAndObjects()
        {
            var containerName = "TestContainer";
            var validObjectJson = @"[
                                        {
                                            ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                            ""last_modified"": ""2014-03-27T20:57:11.150910"",
                                            ""bytes"": 0,
                                            ""name"": ""a/"",
                                            ""content_type"": ""application/octet-stream""
                                        },
                                        {
                                            ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                            ""last_modified"": ""2014-03-27T20:57:36.676350"",
                                            ""bytes"": 0,
                                            ""name"": ""a/b/"",
                                            ""content_type"": ""application/octet-stream""
                                        },
                                        {
                                            ""hash"": ""437b930db84b8079c2dd804a71936b5f"",
                                            ""last_modified"": ""2014-03-27T20:58:36.676620"",
                                            ""bytes"": 9,
                                            ""name"": ""a/b/b"",
                                            ""content_type"": ""text/plain;charset=UTF-8""
                                        },
                                        {
                                            ""hash"": ""437b930db84b8079c2dd804a71936b5f"",
                                            ""last_modified"": ""2014-03-27T20:58:43.935540"",
                                            ""bytes"": 9,
                                            ""name"": ""a/b/c"",
                                            ""content_type"": ""text/plain;charset=UTF-8""
                                        },
                                        {
                                            ""hash"": ""437b930db84b8079c2dd804a71936b5f"",
                                            ""last_modified"": ""2014-03-27T20:58:54.142580"",
                                            ""bytes"": 9,
                                            ""name"": ""a/b/c/object3"",
                                            ""content_type"": ""text/plain;charset=UTF-8""
                                        },
                                        {
                                            ""hash"": ""437b930db84b8079c2dd804a71936b5f"",
                                            ""last_modified"": ""2014-03-27T20:58:25.771530"",
                                            ""bytes"": 9,
                                            ""name"": ""a/object2"",
                                            ""content_type"": ""text/plain;charset=UTF-8""
                                        },
                                        {
                                            ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                            ""last_modified"": ""2014-03-27T20:57:47.122360"",
                                            ""bytes"": 0,
                                            ""name"": ""a/x/"",
                                            ""content_type"": ""application/octet-stream""
                                        },
                                        {
                                            ""hash"": ""437b930db84b8079c2dd804a71936b5f"",
                                            ""last_modified"": ""2014-03-27T20:58:15.696360"",
                                            ""bytes"": 9,
                                            ""name"": ""object1"",
                                            ""content_type"": ""text/plain;charset=UTF-8""
                                        }
                                    ]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "45"},
                {"X-Container-Object-Count", "8"}
            };

            var container = converter.Convert(containerName, headers, validObjectJson);
            Assert.IsNotNull(container);
            Assert.AreEqual(containerName, container.Name);
            Assert.AreEqual(45, container.TotalBytesUsed);
            Assert.AreEqual(8, container.TotalObjectCount);
            Assert.AreEqual(8, container.Objects.ToList().Count());
            Assert.IsTrue(container.Objects.ToList().Any(o => o.Name == "object1"));

            var folders = container.Folders.ToList();
            Assert.AreEqual(1, folders.Count());

            var aNode = folders.First();
            Assert.AreEqual("a", aNode.Name);
            Assert.AreEqual(2, aNode.Folders.Count);
            Assert.AreEqual(1, aNode.Objects.Count);
            Assert.IsTrue(aNode.Objects.Any(f => f.FullName == "a/object2"));

            var xNode = aNode.Folders.First(f => f.Name == "x");
            Assert.AreEqual(0, xNode.Folders.Count);
            Assert.AreEqual(0, xNode.Objects.Count);

            var bNode = aNode.Folders.First(f => f.Name == "b");
            Assert.AreEqual(1, bNode.Folders.Count);
            Assert.AreEqual(2, bNode.Objects.Count);
            Assert.IsTrue(bNode.Folders.Any(f => f.Name == "c"));
            Assert.IsTrue(bNode.Objects.Any(f => f.FullName == "a/b/c"));
            Assert.IsTrue(bNode.Objects.Any(f => f.FullName == "a/b/b"));

            var cNode = bNode.Folders.First(f => f.Name == "c");
            Assert.AreEqual(0, cNode.Folders.Count);
            Assert.AreEqual(1, cNode.Objects.Count);
            Assert.IsTrue(cNode.Objects.Any(f => f.FullName == "a/b/c/object3"));

        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseContainerWithMissingBytesUsedHeader()
        {
            var containerName = "TestContainer";
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Object-Count", "1"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseContainerWithMissingObjectCountHeader()
        {
            var containerName = "TestContainer";
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseContainerWithBadBytesUsedHeader()
        {
            var containerName = "TestContainer";
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "This is not a number"},
                {"X-Container-Object-Count", "1"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseContainerWithBadObjectCountHeader()
        {
            var containerName = "TestContainer";
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "This is not a number"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseContainerWithBadPayload()
        {
            var containerName = "TestContainer";
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"":";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "1"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseContainerWithNullName()
        {
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "1"}
            };

            converter.Convert(null, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseContainerWithNullHeaders()
        {
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                                ""bytes"": 0,
                                                ""name"": ""BLAH"",
                                                ""content_type"": ""application/octet-stream""
                                            }]";

            var converter = new StorageContainerPayloadConverter(new ServiceLocator());

            converter.Convert("Name", null, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseContainerWithNullPayload()
        {
            var converter = new StorageContainerPayloadConverter(new ServiceLocator());
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "1"}
            };

            converter.Convert("Name", headers, null);
        }
    }
}
