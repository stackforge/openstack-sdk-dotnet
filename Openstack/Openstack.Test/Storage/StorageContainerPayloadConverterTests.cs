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
using Openstack.Common.Http;
using Openstack.Storage;

namespace Openstack.Test.Storage
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

            var converter = new StorageContainerPayloadConverter();
            var containers = converter.Convert(validMultipleContainerJson).ToList();

            Assert.AreEqual(2, containers.Count());
            var obj1 =
                containers.First(o => string.Compare(o.Name, "TestContainer", StringComparison.InvariantCultureIgnoreCase) == 0);
            var obj2 =
                containers.First(o => string.Compare(o.Name, "OtherTestContainer", StringComparison.InvariantCultureIgnoreCase) == 0);
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

            var converter = new StorageContainerPayloadConverter();
            var containers = converter.Convert(validSingleContainerJson).ToList();

            Assert.AreEqual(1, containers.Count());
            var obj1 =
                containers.First(o => string.Compare(o.Name, "TestContainer", StringComparison.InvariantCultureIgnoreCase) == 0);
            Assert.IsNotNull(obj1);

            Assert.AreEqual(7, obj1.TotalBytesUsed);
            Assert.AreEqual("TestContainer", obj1.Name);
            Assert.AreEqual(1, obj1.TotalObjectCount);
        }

        [TestMethod]
        public void CanParseValidEmptyJsonArrayPayload()
        {
            var emptyJsonArray = @"[]";

            var converter = new StorageContainerPayloadConverter();
            var containers = converter.Convert(emptyJsonArray).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyPayload()
        {
            var payload = string.Empty;

            var converter = new StorageContainerPayloadConverter();
            var containers = converter.Convert(payload).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullPayload()
        {
            var converter = new StorageContainerPayloadConverter();
            converter.Convert(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotParseInvalidJsonPayload()
        {
            var converter = new StorageContainerPayloadConverter();
            converter.Convert("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotParseInvalidPayload()
        {
            var converter = new StorageContainerPayloadConverter();
            converter.Convert("NOT JSON");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotParseJsonPayloadWithMissingBytesProperty()
        {
            var InvalidJsonWithoutBytes = @"[
                                            {
                                                  ""count"": 1,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter();
            converter.Convert(InvalidJsonWithoutBytes);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotParseJsonPayloadWithMissingCountProperty()
        {
            string InvalidJsonWithoutCount = @"[
                                            {
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter();
            converter.Convert(InvalidJsonWithoutCount);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotParseJsonPayloadWithMissingNameProperty()
        {
            string InvalidJsonWithoutName = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7
                                            }]";

            var converter = new StorageContainerPayloadConverter();
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

            var converter = new StorageContainerPayloadConverter();
            try
            {
                converter.Convert(InvalidJsonWithoutBytes);
                Assert.Fail("Parsing did not fail as expected.");
            }
            catch (HttpParseException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Storage Container 'TestContainer'"));
            }

        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotParseJsonPayloadWithBadBytesValue()
        {
            string InvalidJsonWithBadBytesValue = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": ""NOT A NUMBER"",
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter();
            converter.Convert(InvalidJsonWithBadBytesValue);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotParseJsonPayloadWithBadCountValue()
        {
            string InvalidJsonWithBadCountValue = @"[
                                            {
                                                  ""count"": ""NOT A NUMBER"",
                                                  ""bytes"": 12345,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageContainerPayloadConverter();
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

            var converter = new StorageContainerPayloadConverter();
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
        [ExpectedException(typeof(HttpParseException))]
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

            var converter = new StorageContainerPayloadConverter();
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Object-Count", "1"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
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

            var converter = new StorageContainerPayloadConverter();
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
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

            var converter = new StorageContainerPayloadConverter();
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "This is not a number"},
                {"X-Container-Object-Count", "1"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
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

            var converter = new StorageContainerPayloadConverter();
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "This is not a number"}
            };

            converter.Convert(containerName, headers, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpParseException))]
        public void CannotParseContainerWithBadPayload()
        {
            var containerName = "TestContainer";
            var validObjectJson = @"[
                                            {
                                                ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                                ""last_modified"":";
            
            var converter = new StorageContainerPayloadConverter();
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

            var converter = new StorageContainerPayloadConverter();
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

            var converter = new StorageContainerPayloadConverter();

            converter.Convert("Name", null, validObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseContainerWithNullPayload()
        {
            var converter = new StorageContainerPayloadConverter();
            var headers = new HttpHeadersAbstraction
            {
                {"X-Container-Bytes-Used", "12345"},
                {"X-Container-Object-Count", "1"}
            };

            converter.Convert("Name", headers, null);
        }
    }
}
