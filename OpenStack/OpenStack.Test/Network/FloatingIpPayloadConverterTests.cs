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
    public class FloatingIpPayloadConverterTests
    {
        internal string CreateFloatingIpJsonFixtrue(string id, string ipAddress, FloatingIpStatus status)
        {
            var payloadFixture = @"{{
                    ""router_id"": ""fafac59b-a94a-4525-8700-f4f448e0ac97"",
                    ""status"": ""{1}"",
                    ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                    ""floating_network_id"": ""3eaab3f7-d3f2-430f-aa73-d07f39aae8f4"",
                    ""fixed_ip_address"": ""10.0.0.2"",
                    ""floating_ip_address"": ""{2}"",
                    ""port_id"": ""9da94672-6e6b-446c-9579-3dd5484b31fd"",
                    ""id"": ""{0}""
                }}";

            return string.Format(payloadFixture, id, status, ipAddress);
        }

        [TestMethod]
        public void CanParseValidFloatingIpsJsonPayloadWithMultipleIps()
        {
            var validMultipleIpJsonFixture = @"{{ ""floatingips"": [ {0} ] }}";
            var firstIp = CreateFloatingIpJsonFixtrue("12345", "172.0.0.1", FloatingIpStatus.Active);
            var secondIp = CreateFloatingIpJsonFixtrue("54321", "172.0.0.2", FloatingIpStatus.Down);

            var validMultipleIpsJson = string.Format(validMultipleIpJsonFixture,
                string.Join(",", new List<string>() { firstIp, secondIp }));

            var converter = new FloatingIpPayloadConverter();
            var floatingIps = converter.ConvertFloatingIps(validMultipleIpsJson).ToList();

            Assert.AreEqual(2, floatingIps.Count());
            var ip1 =
                floatingIps.First(o => string.Equals(o.Id, "12345", StringComparison.InvariantCultureIgnoreCase));
            var ip2 =
                floatingIps.First(o => string.Equals(o.Id, "54321", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(ip1);
            Assert.IsNotNull(ip2);

            Assert.AreEqual("12345", ip1.Id);
            Assert.AreEqual("172.0.0.1", ip1.FloatingIpAddress);
            Assert.AreEqual(FloatingIpStatus.Active, ip1.Status);

            Assert.AreEqual("54321", ip2.Id);
            Assert.AreEqual("172.0.0.2", ip2.FloatingIpAddress);
            Assert.AreEqual(FloatingIpStatus.Down, ip2.Status);
        }

        [TestMethod]
        public void CanConvertValidFloatingIpsJsonPayloadWithSingleNetwork()
        {
            var validSingleIpJsonPayload = @"{{ ""floatingips"": [ {0} ] }}";
            var firstIp = CreateFloatingIpJsonFixtrue("12345", "172.0.0.1", FloatingIpStatus.Active);
            
            var validSingleIpPayload = string.Format(validSingleIpJsonPayload,
                string.Join(",", new List<string>() { firstIp }));

            var converter = new FloatingIpPayloadConverter();
            var floatingIps = converter.ConvertFloatingIps(validSingleIpPayload).ToList();

            Assert.AreEqual(1, floatingIps.Count());
            var ip1 =
                floatingIps.First(o => string.Equals(o.Id, "12345", StringComparison.InvariantCultureIgnoreCase));
            
            Assert.IsNotNull(ip1);

            Assert.AreEqual("12345", ip1.Id);
            Assert.AreEqual("172.0.0.1", ip1.FloatingIpAddress);
            Assert.AreEqual(FloatingIpStatus.Active, ip1.Status);
        }

        [TestMethod]
        public void CanParseValidFloatingIpsPayloadWithEmptyJsonArray()
        {
            var emptyJsonArray = @"{ ""floatingips"": [ ] }";

            var converter = new FloatingIpPayloadConverter();
            var networks = converter.ConvertFloatingIps(emptyJsonArray).ToList();

            Assert.AreEqual(0, networks.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyFloatingIpsPayload()
        {
            var payload = string.Empty;

            var converter = new FloatingIpPayloadConverter();
            var networks = converter.ConvertFloatingIps(payload).ToList();

            Assert.AreEqual(0, networks.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullFloatingIpsPayload()
        {
            var converter = new FloatingIpPayloadConverter();
            converter.ConvertFloatingIps(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidFloatingIpsJsonPayload()
        {
            var converter = new FloatingIpPayloadConverter();
            converter.ConvertFloatingIps("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidFloatingIpsPayload()
        {
            var converter = new FloatingIpPayloadConverter();
            converter.ConvertFloatingIps("NOT JSON");
        }

        [TestMethod]
        public void CanConvertValidFloatingIpJsonPayload()
        {
            var validSingleIpJsonPayload = @"{{ ""floatingip"": {0} }}";
            var firstIp = CreateFloatingIpJsonFixtrue("12345", "172.0.0.1", FloatingIpStatus.Active);

            var validSingleIpPayload = string.Format(validSingleIpJsonPayload,
                string.Join(",", new List<string>() { firstIp }));

            var converter = new FloatingIpPayloadConverter();
            var ip1 = converter.Convert(validSingleIpPayload);

            Assert.IsNotNull(ip1);
            Assert.AreEqual("12345", ip1.Id);
            Assert.AreEqual("172.0.0.1", ip1.FloatingIpAddress);
            Assert.AreEqual(FloatingIpStatus.Active, ip1.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullFloatingIpPayload()
        {
            var converter = new FloatingIpPayloadConverter();
            converter.Convert(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidFloatingIpJsonPayload()
        {
            var converter = new FloatingIpPayloadConverter();
            converter.Convert("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidFloatingIpPayload()
        {
            var converter = new FloatingIpPayloadConverter();
            converter.Convert("NOT JSON");
        }
    }
}
