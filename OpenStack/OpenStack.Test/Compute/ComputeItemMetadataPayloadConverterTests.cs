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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    [TestClass]
    public class ComputeItemMetadataPayloadConverterTests
    {
        [TestMethod]
        public void CanConvertJsonPayloadToMetadata()
        {
            var metadataPayload = @"{
                                        ""metadata"": {
                                            ""item1"": ""value1"",
                                            ""item2"": ""value2""
                                        }
                                    }";

            var converter = new ComputeItemMetadataPayloadConverter();
            var metadata = converter.Convert(metadataPayload);

            Assert.AreEqual(2, metadata.Count);
            Assert.IsTrue(metadata.ContainsKey("item1"));
            Assert.AreEqual("value1",metadata["item1"]);
            Assert.IsTrue(metadata.ContainsKey("item2"));
            Assert.AreEqual("value2", metadata["item2"]);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadEmptyObjectToMetadata()
        {
            var emptyObjectFixture = @"{ }";

            var converter = new ComputeItemMetadataPayloadConverter();
            converter.Convert(emptyObjectFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectJsonToMetadata()
        {
            var nonObjectJson = @"[]";

            var converter = new ComputeItemMetadataPayloadConverter();
            converter.Convert(nonObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidJsonToMetadata()
        {
            var badJsonFixture = @"{ NOT JSON";

            var converter = new ComputeItemMetadataPayloadConverter();
            converter.Convert(badJsonFixture);
        }

        [TestMethod]
        public void CanConvertEmptyMetadataToJson()
        {
            var metadata = new Dictionary<string, string>();

            var converter = new ComputeItemMetadataPayloadConverter();
            var payload = converter.Convert(metadata);

            var jsonObj = JObject.Parse(payload);
            var metadataToken = jsonObj["metadata"];
            Assert.IsNotNull(metadataToken);
            Assert.AreEqual(0, metadataToken.Children().Count());
        }

        [TestMethod]
        public void CanConvertSingleMetadataToJson()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" } };

            var converter = new ComputeItemMetadataPayloadConverter();
            var payload = converter.Convert(metadata);

            var jsonObj = JObject.Parse(payload);
            var metadataToken = jsonObj["metadata"];
            Assert.IsNotNull(metadataToken);
            Assert.AreEqual("value1", metadataToken["item1"]);
        }

        [TestMethod]
        public void CanConvertMultipleMetadataToJson()
        {
            var metadata = new Dictionary<string, string>() {{"item1", "value1"}, {"item2", "value2"}};

            var converter = new ComputeItemMetadataPayloadConverter();
            var payload = converter.Convert(metadata);

            var jsonObj = JObject.Parse(payload);
            var metadataToken = jsonObj["metadata"];
            Assert.IsNotNull(metadataToken);
            Assert.AreEqual("value1", metadataToken["item1"]);
            Assert.AreEqual("value2", metadataToken["item2"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertNullMetadataToJson()
        {
            IDictionary<string, string> metadata = null;
            var converter = new ComputeItemMetadataPayloadConverter();
            converter.Convert(metadata);
        }
    }
}
