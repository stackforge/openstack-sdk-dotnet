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
using OpenStack.Network;

namespace OpenStack.Test.Network
{
    [TestClass]
    public class NetworkPayloadConverterTests
    {
        internal string CreateNetworkJsonFixtrue(string id, string name, NetworkStatus status)
        {
            var NetworkJsonResponseFixture = @"{{
                ""status"": ""{2}"",
                ""subnets"": [
                    ""d3839504-ec4c-47a4-b7c7-07af079a48bb""
                ],
                ""name"": ""{1}"",
                ""router:external"": false,
                ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                ""admin_state_up"": true,
                ""shared"": false,
                ""id"": ""{0}""
            }}";

            return string.Format(NetworkJsonResponseFixture, id, name, status);
        }

        [TestMethod]
        public void CanParseValidNetworksJsonPayloadWithMultipleNetworks()
        {
            var validMultipleNetworkJsonFixture = @"{{ ""networks"": [ {0} ] }}";
            var firstNetwork = CreateNetworkJsonFixtrue("12345", "MyNetwork", NetworkStatus.Active);
            var secondNetwork = CreateNetworkJsonFixtrue("54321", "NetworkMy", NetworkStatus.Down);

            var validMultipleNetworksJson = string.Format(validMultipleNetworkJsonFixture,
                string.Join(",", new List<string>() { firstNetwork, secondNetwork }));

            var converter = new NetworkPayloadConverter();
            var networks = converter.ConvertNetworks(validMultipleNetworksJson).ToList();

            Assert.AreEqual(2, networks.Count());
            var ntw1 =
                networks.First(o => string.Equals(o.Id, "12345", StringComparison.InvariantCultureIgnoreCase));
            var ntw2 =
                networks.First(o => string.Equals(o.Id, "54321", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(ntw1);
            Assert.IsNotNull(ntw2);

            Assert.AreEqual("12345", ntw1.Id);
            Assert.AreEqual("MyNetwork", ntw1.Name);
            Assert.AreEqual(NetworkStatus.Active, ntw1.Status);

            Assert.AreEqual("54321", ntw2.Id);
            Assert.AreEqual("NetworkMy", ntw2.Name);
            Assert.AreEqual(NetworkStatus.Down, ntw2.Status);
        }

        [TestMethod]
        public void CanConvertValidNetworksJsonPayloadWithSingleNetwork()
        {
            var validMultipleNetworkJsonFixture = @"{{ ""networks"": [ {0} ] }}";
            var firstNetwork = CreateNetworkJsonFixtrue("12345", "MyNetwork", NetworkStatus.Active);
            
            var validMultipleNetworksJson = string.Format(validMultipleNetworkJsonFixture,
                string.Join(",", new List<string>() { firstNetwork }));

            var converter = new NetworkPayloadConverter();
            var networks = converter.ConvertNetworks(validMultipleNetworksJson).ToList();

            Assert.AreEqual(1, networks.Count());
            var ntw1 =
                networks.First(o => string.Equals(o.Id, "12345", StringComparison.InvariantCultureIgnoreCase));
            
            Assert.IsNotNull(ntw1);

            Assert.AreEqual("12345", ntw1.Id);
            Assert.AreEqual("MyNetwork", ntw1.Name);
            Assert.AreEqual(NetworkStatus.Active, ntw1.Status);
        }

        [TestMethod]
        public void CanParseValidNetworksPayloadWithEmptyJsonArray()
        {
            var emptyJsonArray = @"{ ""networks"": [ ] }";

            var converter = new NetworkPayloadConverter();
            var networks = converter.ConvertNetworks(emptyJsonArray).ToList();

            Assert.AreEqual(0, networks.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyNetworksPayload()
        {
            var payload = string.Empty;

            var converter = new NetworkPayloadConverter();
            var networks = converter.ConvertNetworks(payload).ToList();

            Assert.AreEqual(0, networks.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullNetworksPayload()
        {
            var converter = new NetworkPayloadConverter();
            converter.ConvertNetworks(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidNetworksJsonPayload()
        {
            var converter = new NetworkPayloadConverter();
            converter.ConvertNetworks("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidNetworksPayload()
        {
            var converter = new NetworkPayloadConverter();
            converter.ConvertNetworks("NOT JSON");
        }
    }
}
