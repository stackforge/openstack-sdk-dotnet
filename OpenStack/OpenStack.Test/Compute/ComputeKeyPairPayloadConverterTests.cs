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
    public class ComputeKeyPairPayloadConverterTests
    {
        internal string CreateKeyPairJsonFixtrue(string name, string publickKey, string fingerprint)
        {
            var ComputeKeyPairJsonResponseFixture = @"{{
                ""keypair"": {{
                    ""public_key"": ""{1}"",
                    ""name"": ""{0}"",
                    ""fingerprint"": ""{2}""
                }}
            }}";

            return string.Format(ComputeKeyPairJsonResponseFixture, name, publickKey, fingerprint);
        }

        [TestMethod]
        public void CanConvertJsonPayloadToKeyPair()
        {
            var keyName = "Key1";
            var publicKey = "12345";
            var fingerprint = "abcdef";

            var computeFlavorJsonResponseFixture = CreateKeyPairJsonFixtrue(keyName, publicKey, fingerprint);

            var converter = new ComputeKeyPairPayloadConverter();
            var keyPair = converter.Convert(computeFlavorJsonResponseFixture);
            Assert.IsNotNull(keyPair);
            Assert.AreEqual(keyName, keyPair.Name);
            Assert.AreEqual(publicKey, keyPair.PublicKey);
            Assert.AreEqual(fingerprint, keyPair.Fingerprint);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingPublicKey()
        {
            var missingFixture = @"{
                ""keypair"": {
                    ""name"": ""Key1"",
                    ""fingerprint"": ""ABCDEF""
                }
            }";

            var converter = new ComputeKeyPairPayloadConverter();
            var keyPair = converter.Convert(missingFixture);
            Assert.IsNotNull(keyPair);
            Assert.AreEqual("Key1", keyPair.Name);
            Assert.AreEqual(string.Empty, keyPair.PublicKey);
            Assert.AreEqual("ABCDEF", keyPair.Fingerprint);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingFingerprint()
        {
            var missingFixture = @"{
                ""keypair"": {
                    ""public_key"": ""12345"",
                    ""name"": ""Key1""
                }
            }";

            var converter = new ComputeKeyPairPayloadConverter();
            var keyPair = converter.Convert(missingFixture);
            Assert.IsNotNull(keyPair);
            Assert.AreEqual("Key1", keyPair.Name);
            Assert.AreEqual("12345", keyPair.PublicKey);
            Assert.AreEqual(string.Empty, keyPair.Fingerprint);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingName()
        {
            var missingFixture = @"{
                ""keypair"": {
                    ""public_key"": ""12345"",
                    ""fingerprint"": ""ABCDEF""
                }
            }";

            var converter = new ComputeKeyPairPayloadConverter();
            converter.Convert(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadEmptyObjectToKeyPair()
        {
            var emptyObjectFixture = @"{ }";

            var converter = new ComputeKeyPairPayloadConverter();
            converter.ConvertKeyPairs(emptyObjectFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidJsonToKeyPair()
        {
            var badJsonFixture = @"{ NOT JSON";

            var converter = new ComputeKeyPairPayloadConverter();
            converter.ConvertKeyPairs(badJsonFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectJsonToKeyPair()
        {
            var nonObjectJson = @"[]";

            var converter = new ComputeKeyPairPayloadConverter();
            converter.ConvertKeyPairs(nonObjectJson);
        }

        [TestMethod]
        public void CanParseValidJsonPayloadWithMultipleKeyPairs()
        {
            var validMultipleKeyPairsJsonFixture = @"{{ ""keypairs"": [ {0} ] }}";
            var firstKey = CreateKeyPairJsonFixtrue("Key1", "12345", "abcdef");
            var secondKey = CreateKeyPairJsonFixtrue("Key2", "54321", "fedcba");

            var validMultipleKeyPairsJson = string.Format(validMultipleKeyPairsJsonFixture,
                string.Join(",", new List<string>() {firstKey, secondKey}));

            var converter = new ComputeKeyPairPayloadConverter();
            var pairs = converter.ConvertKeyPairs(validMultipleKeyPairsJson).ToList();

            Assert.AreEqual(2, pairs.Count());
            var key1 =
                pairs.First(o => string.Equals(o.Name, "Key1", StringComparison.InvariantCultureIgnoreCase));
            var key2 =
                pairs.First(o => string.Equals(o.Name, "Key2", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(key1);
            Assert.IsNotNull(key2);

            Assert.AreEqual("12345", key1.PublicKey);
            Assert.AreEqual("abcdef", key1.Fingerprint);

            Assert.AreEqual("54321", key2.PublicKey);
            Assert.AreEqual("fedcba", key2.Fingerprint);
        }

        [TestMethod]
        public void CanConvertValidJsonPayloadWithSingleKeyPair()
        {
            var validKeyPairsJsonFixture = @"{{ ""keypairs"": [ {0} ] }}";
            var firstKey = CreateKeyPairJsonFixtrue("Key1", "12345", "abcdef");

            var validKeyPairsJson = string.Format(validKeyPairsJsonFixture, firstKey);

            var converter = new ComputeKeyPairPayloadConverter();
            var pairs = converter.ConvertKeyPairs(validKeyPairsJson).ToList();

            Assert.AreEqual(1, pairs.Count());
            var key1 = pairs.First(o => string.Equals(o.Name, "Key1", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(key1);

            Assert.AreEqual("12345", key1.PublicKey);
            Assert.AreEqual("abcdef", key1.Fingerprint);
        }

        [TestMethod]
        public void CanParseValidKeyPairsPayloadWithEmptyJsonArray()
        {
            var emptyJsonArray = @"{ ""keypairs"": [ ] }";

            var converter = new ComputeKeyPairPayloadConverter();
            var containers = converter.ConvertKeyPairs(emptyJsonArray).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyKeyPairsPayload()
        {
            var payload = string.Empty;

            var converter = new ComputeKeyPairPayloadConverter();
            var containers = converter.ConvertKeyPairs(payload).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullKeyPairsPayload()
        {
            var converter = new ComputeKeyPairPayloadConverter();
            converter.ConvertKeyPairs(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidKeyPairsJsonPayload()
        {
            var converter = new ComputeKeyPairPayloadConverter();
            converter.ConvertKeyPairs("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidKeyPairsPayload()
        {
            var converter = new ComputeKeyPairPayloadConverter();
            converter.ConvertKeyPairs("NOT JSON");
        }
    }
}
