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
    public class ComputeServerPayloadConverterTests
    {
        internal string CreateServerJsonFixtrue(string name, string id, string status, int progress, string publicUri, string permUri)
        {
            var payloadFixture = @"{{
                ""server"": {{
                    ""status"": ""{2}"",
                    ""updated"": ""2014-06-11T18:04:46Z"",
                    ""hostId"": ""bd5417ccb076908f6e0d639c37c053b0b6b9681db3464d19908dd4d9"",
                    ""addresses"": {{
                        ""private"": [
                            {{
                                ""OS-EXT-IPS-MAC:mac_addr"": ""fa:16:3e:34:da:44"",
                                ""version"": 4,
                                ""addr"": ""10.0.0.2"",
                                ""OS-EXT-IPS:type"": ""fixed""
                            }},
                            {{
                                ""OS-EXT-IPS-MAC:mac_addr"": ""fa:16:3e:34:da:44"",
                                ""version"": 4,
                                ""addr"": ""172.24.4.3"",
                                ""OS-EXT-IPS:type"": ""floating""
                            }}
                        ]
                    }},
                    ""links"": [
                        {{
                            ""href"": ""{4}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{5}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""key_name"": null,
                    ""image"": {{
                        ""id"": ""c650e788-3c46-4efc-bfa6-1d94a14d6405"",
                        ""links"": [
                            {{
                                ""href"": ""http://15.125.87.81:8774/ffe683d1060449d09dac0bf9d7a371cd/images/c650e788-3c46-4efc-bfa6-1d94a14d6405"",
                                ""rel"": ""bookmark""
                            }}
                        ]
                    }},
                    ""OS-EXT-STS:task_state"": null,
                    ""OS-EXT-STS:vm_state"": ""active"",
                    ""OS-SRV-USG:launched_at"": ""2014-06-11T18:04:45.000000"",
                    ""flavor"": {{
                        ""id"": ""1"",
                        ""links"": [
                            {{
                                ""href"": ""http://15.125.87.81:8774/ffe683d1060449d09dac0bf9d7a371cd/flavors/1"",
                                ""rel"": ""bookmark""
                            }}
                        ]
                    }},
                    ""id"": ""{0}"",
                    ""security_groups"": [
                        {{
                            ""name"": ""MyGroup""
                        }},
                        {{
                            ""name"": ""default""
                        }}
                    ],
                    ""OS-SRV-USG:terminated_at"": null,
                    ""OS-EXT-AZ:availability_zone"": ""nova"",
                    ""user_id"": ""70d48d344b494a1cbe8adbf7c02be7b5"",
                    ""name"": ""{1}"",
                    ""created"": ""2014-06-11T18:04:25Z"",
                    ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                    ""OS-DCF:diskConfig"": ""AUTO"",
                    ""os-extended-volumes:volumes_attached"": [],
                    ""accessIPv4"": """",
                    ""accessIPv6"": """",
                    ""progress"": {3},
                    ""OS-EXT-STS:power_state"": 1,
                    ""config_drive"": """",
                    ""metadata"": {{}}
                }}
            }}";
            return string.Format(payloadFixture, id, name, status, progress, publicUri, permUri);
        }

        internal string CreateServerRequstSummaryJsonFixtrue(string id, string permaUri, string publicUri, string adminPass)
        {
            var computeServerSummaryJsonResponseFixture = @"{{
                ""server"": {{
                    ""security_groups"": [
                        {{
                            ""name"": ""default""
                        }},
                        {{
                            ""name"": ""MyGroup""
                        }}
                    ],
                    ""OS-DCF:diskConfig"": ""MANUAL"",
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
                    ""adminPass"": ""{3}""
                }}
            }}";

            return string.Format(computeServerSummaryJsonResponseFixture, id, publicUri, permaUri, adminPass);
        }

        internal string CreateServersSummaryJsonFixtrue(string name, string id, string permaUri, string publicUri)
        {
            var computeServerSummaryJsonResponseFixture = @"{{
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

            return string.Format(computeServerSummaryJsonResponseFixture, id, publicUri, permaUri, name);
        }

        [TestMethod]
        public void CanConvertJsonPayloadToServer()
        {
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverId = "12345";
            var serverName = "testServer";
            var serverProgress = 0;
            var serverState = "ACTIVE";

            var computeServerFixture = CreateServerJsonFixtrue(serverName, serverId, serverState, serverProgress, serverPublicUri, serverPermUri);

            var converter = new ComputeServerPayloadConverter();
            var server = converter.Convert(computeServerFixture);

            Assert.IsNotNull(server);
            Assert.AreEqual(serverId, server.Id);
            Assert.AreEqual(serverName, server.Name);
            Assert.AreEqual(serverState.ToLower(), server.Status.ToString().ToLower());
            Assert.AreEqual(serverProgress, server.Progress);
            Assert.AreEqual(new Uri(serverPermUri), server.PermanentUri);
            Assert.AreEqual(new Uri(serverPublicUri), server.PublicUri);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingIdToServer()
        {
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverName = "testServer";
            var serverProgress = 0;
            var serverState = "ACTIVE";

            var payloadFixture = @"{{
                ""server"": {{
                    ""status"": ""{1}"",
                    ""updated"": ""2014-06-11T18:04:46Z"",
                    ""links"": [
                        {{
                            ""href"": ""{3}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{4}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""name"": ""{0}"",
                    ""created"": ""2014-06-11T18:04:25Z"",
                    ""progress"": {2}
                }}
            }}";

            var missingFixture = string.Format(payloadFixture, serverName, serverState, serverProgress, serverPublicUri, serverPermUri);

            var converter = new ComputeServerPayloadConverter();
            converter.Convert(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingpublicUriToServer()
        {
            var serverPermUri = "http://www.server.com/servers/1";
            var serverId = "12345";
            var serverName = "testServer";
            var serverProgress = 0;
            var serverState = "ACTIVE";

            var payloadFixture = @"{{
                ""server"": {{
                    ""status"": ""{2}"",
                    ""updated"": ""2014-06-11T18:04:46Z"",
                    ""links"": [
                        {{
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{4}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""id"": ""{0}"",
                    ""name"": ""{1}"",
                    ""created"": ""2014-06-11T18:04:25Z"",
                    ""progress"": {3}
                }}
            }}";

            var missingFixture = string.Format(payloadFixture, serverId, serverName, serverState, serverProgress, serverPermUri);

            var converter = new ComputeServerPayloadConverter();
            converter.Convert(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingPermUriToServer()
        {
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverId = "12345";
            var serverName = "testServer";
            var serverProgress = 0;
            var serverState = "ACTIVE";

            var payloadFixture = @"{{
                ""server"": {{
                    ""status"": ""{2}"",
                    ""updated"": ""2014-06-11T18:04:46Z"",
                    ""links"": [
                        {{
                            ""href"": ""{4}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""id"": ""{0}"",
                    ""name"": ""{1}"",
                    ""created"": ""2014-06-11T18:04:25Z"",
                    ""progress"": {3}
                }}
            }}";

            var missingFixture = string.Format(payloadFixture, serverId, serverName, serverState, serverProgress, serverPublicUri);

            var converter = new ComputeServerPayloadConverter();
            converter.Convert(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingStatusToServer()
        {
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverId = "12345";
            var serverName = "testServer";
            var serverProgress = 0;

            var payloadFixture = @"{{
                ""server"": {{
                    ""updated"": ""2014-06-11T18:04:46Z"",
                    ""links"": [
                        {{
                            ""href"": ""{3}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{4}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""id"": ""{0}"",
                    ""name"": ""{1}"",
                    ""created"": ""2014-06-11T18:04:25Z"",
                    ""progress"": {2}
                }}
            }}";

            var missingFixture = string.Format(payloadFixture, serverId, serverName, serverProgress, serverPublicUri, serverPermUri);

            var converter = new ComputeServerPayloadConverter();
            converter.Convert(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingNameToServer()
        {
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverId = "12345";
            var serverProgress = 0;
            var serverState = "ACTIVE";

            var payloadFixture = @"{{
                ""server"": {{
                    ""status"": ""{1}"",
                    ""updated"": ""2014-06-11T18:04:46Z"",
                    ""links"": [
                        {{
                            ""href"": ""{3}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{4}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""id"": ""{0}"",
                    ""created"": ""2014-06-11T18:04:25Z"",
                    ""progress"": {2}
                }}
            }}";

            var missingFixture = string.Format(payloadFixture, serverId, serverState, serverProgress, serverPublicUri, serverPermUri);

            var converter = new ComputeServerPayloadConverter();
            converter.Convert(missingFixture);
        }

        [TestMethod]
        public void CanConvertJsonPayloadMissingProgressToServer()
        {
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverId = "12345";
            var serverName = "testServer";
            var serverState = "ACTIVE";

            var payloadFixture = @"{{
                ""server"": {{
                    ""status"": ""{2}"",
                    ""updated"": ""2014-06-11T18:04:46Z"",
                    ""links"": [
                        {{
                            ""href"": ""{3}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{4}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""id"": ""{0}"",
                    ""name"": ""{1}"",
                    ""created"": ""2014-06-11T18:04:25Z""
                }}
            }}";

            var missingFixture = string.Format(payloadFixture, serverId, serverName, serverState, serverPublicUri, serverPermUri);

            var converter = new ComputeServerPayloadConverter();
            var server = converter.Convert(missingFixture);

            Assert.IsNotNull(server);
            Assert.AreEqual(serverId, server.Id);
            Assert.AreEqual(serverName, server.Name);
            Assert.AreEqual(serverState.ToLower(), server.Status.ToString().ToLower());
            Assert.AreEqual(0, server.Progress);
            Assert.AreEqual(new Uri(serverPermUri), server.PermanentUri);
            Assert.AreEqual(new Uri(serverPublicUri), server.PublicUri);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertServerJsonPayloadEmptyObjectToServer()
        {
            var emptyObjectFixture = @"{ }";

            var converter = new ComputeServerPayloadConverter();
            converter.Convert(emptyObjectFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidServerJsonToServer()
        {
            var badJsonFixture = @"{ NOT JSON";

            var converter = new ComputeServerPayloadConverter();
            converter.Convert(badJsonFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectServerJsonToServer()
        {
            var nonObjectJson = @"[]";

            var converter = new ComputeServerPayloadConverter();
            converter.Convert(nonObjectJson);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertNullJsonToServer()
        {
            var converter = new ComputeServerPayloadConverter();
            converter.Convert(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotConvertEmptyStringJsonToServer()
        {
            var converter = new ComputeServerPayloadConverter();
            converter.Convert(string.Empty);
        }

        [TestMethod]
        public void CanConvertSummaryJsonPayloadToServer()
        {
            
            var serverId = "1";
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverAdminPass = "ABCDE";

            var computeServerFixture = CreateServerRequstSummaryJsonFixtrue(serverId, serverPermUri, serverPublicUri, serverAdminPass);

            var converter = new ComputeServerPayloadConverter();
            var server = converter.ConvertSummary(computeServerFixture);
            
            Assert.IsNotNull(server);
            Assert.AreEqual(serverId, server.Id);
            Assert.AreEqual(serverAdminPass, server.AdminPassword);
            Assert.AreEqual(new Uri(serverPermUri), server.PermanentUri);
            Assert.AreEqual(new Uri(serverPublicUri), server.PublicUri);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSummaryJsonPayloadMissingIdToServer()
        {
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverAdminPass = "ABCDE";

            var computeServerSummaryJsonResponseFixture = @"{{
                ""server"": {{
                    ""security_groups"": [
                        {{
                            ""name"": ""default""
                        }},
                        {{
                            ""name"": ""MyGroup""
                        }}
                    ],
                    ""OS-DCF:diskConfig"": ""MANUAL"",
                    ""links"": [
                        {{
                            ""href"": ""{1}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{0}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""adminPass"": ""{2}""
                }}
            }}";

            var missingFixture = string.Format(computeServerSummaryJsonResponseFixture, serverPermUri, serverPublicUri, serverAdminPass);

            var converter = new ComputeServerPayloadConverter();
            converter.ConvertSummary(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSummaryJsonPayloadMissingpublicUriToServer()
        {
            var serverId = "1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverAdminPass = "ABCDE";

            var computeServerSummaryJsonResponseFixture = @"{{
                ""server"": {{
                    ""security_groups"": [
                        {{
                            ""name"": ""default""
                        }},
                        {{
                            ""name"": ""MyGroup""
                        }}
                    ],
                    ""OS-DCF:diskConfig"": ""MANUAL"",
                    ""id"": ""{0}"",
                    ""links"": [
                        {{
                            ""href"": ""{1}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""adminPass"": ""{2}""
                }}
            }}";

            var missingFixture = string.Format(computeServerSummaryJsonResponseFixture, serverId, serverPermUri, serverAdminPass);

            var converter = new ComputeServerPayloadConverter();
            converter.ConvertSummary(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSummaryJsonPayloadMissingPermUriToServer()
        {
            var serverId = "1";
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverAdminPass = "ABCDE";

            var computeServerSummaryJsonResponseFixture = @"{{
                ""server"": {{
                    ""security_groups"": [
                        {{
                            ""name"": ""default""
                        }},
                        {{
                            ""name"": ""MyGroup""
                        }}
                    ],
                    ""OS-DCF:diskConfig"": ""MANUAL"",
                    ""id"": ""{0}"",
                    ""links"": [
                        {{
                            ""href"": ""{1}"",
                            ""rel"": ""self""
                        }}
                    ],
                    ""adminPass"": ""{2}""
                }}
            }}";

            var missingFixture = string.Format(computeServerSummaryJsonResponseFixture, serverId, serverPublicUri, serverAdminPass);

            var converter = new ComputeServerPayloadConverter();
            converter.ConvertSummary(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertSummaryJsonPayloadMissingAdminPasswordToServer()
        {
            var serverId = "1";
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";

            var computeServerSummaryJsonResponseFixture = @"{{
                ""server"": {{
                    ""security_groups"": [
                        {{
                            ""name"": ""default""
                        }},
                        {{
                            ""name"": ""MyGroup""
                        }}
                    ],
                    ""OS-DCF:diskConfig"": ""MANUAL"",
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
                    ]
                }}
            }}";

            var missingFixture = string.Format(computeServerSummaryJsonResponseFixture, serverId, serverPermUri, serverPublicUri);

            var converter = new ComputeServerPayloadConverter();
            converter.ConvertSummary(missingFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadEmptyObjectToServer()
        {
            var emptyObjectFixture = @"{ }";

            var converter = new ComputeServerPayloadConverter();
            converter.ConvertSummary(emptyObjectFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidJsonToServer()
        {
            var badJsonFixture = @"{ NOT JSON";

            var converter = new ComputeServerPayloadConverter();
            converter.ConvertSummary(badJsonFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectJsonToServer()
        {
            var nonObjectJson = @"[]";

            var converter = new ComputeServerPayloadConverter();
            converter.ConvertSummary(nonObjectJson);
        }

        [TestMethod]
        public void CanParseValidServersJsonPayloadWithMultipleFlavors()
        {
            var validMultipleServersJsonFixture = @"{{ ""servers"": [ {0} ] }}";
            var firstServer = CreateServersSummaryJsonFixtrue("srv1", "1", "http://server.com/servers/1",
                "http://server.com/v2/servers/1");
            var secondServer = CreateServersSummaryJsonFixtrue("srv2", "2", "http://server.com/servers/2",
               "http://server.com/v2/servers/2");

            var validMultipleServersJson = string.Format(validMultipleServersJsonFixture,
                string.Join(",", new List<string>() { firstServer, secondServer }));

            var converter = new ComputeServerPayloadConverter();
            var servers = converter.ConvertServers(validMultipleServersJson).ToList();

            Assert.AreEqual(2, servers.Count());
            var srv1 =
                servers.First(o => string.Equals(o.Name, "srv1", StringComparison.InvariantCultureIgnoreCase));
            var srv2 =
                servers.First(o => string.Equals(o.Name, "srv2", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(srv1);
            Assert.IsNotNull(srv2);

            Assert.AreEqual("1", srv1.Id);
            Assert.AreEqual(new Uri("http://server.com/servers/1"), srv1.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/servers/1"), srv1.PublicUri);

            Assert.AreEqual("2", srv2.Id);
            Assert.AreEqual(new Uri("http://server.com/servers/2"), srv2.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/servers/2"), srv2.PublicUri);
        }

        [TestMethod]
        public void CanConvertValidServersJsonPayloadWithSingleFlavor()
        {
            var validFlavorsJsonFixture = @"{{ ""servers"": [ {0} ] }}";
            var firstServer = CreateServersSummaryJsonFixtrue("myserver", "1", "http://server.com/servers/1",
                "http://server.com/v2/servers/1");
            var validSingleServerPayload = string.Format(validFlavorsJsonFixture, firstServer);

            var converter = new ComputeServerPayloadConverter();
            var servers = converter.ConvertServers(validSingleServerPayload).ToList();

            Assert.AreEqual(1, servers.Count());
            var srv1 =
                servers.First(o => string.Equals(o.Name, "myserver", StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(srv1);

            Assert.AreEqual("1", srv1.Id);
            Assert.AreEqual(new Uri("http://server.com/servers/1"), srv1.PermanentUri);
            Assert.AreEqual(new Uri("http://server.com/v2/servers/1"), srv1.PublicUri);
        }

        [TestMethod]
        public void CanParseValidServersPayloadWithEmptyJsonArray()
        {
            var emptyJsonArray = @"{ ""servers"": [ ] }";

            var converter = new ComputeServerPayloadConverter();
            var containers = converter.ConvertServers(emptyJsonArray).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        public void CanParseAnEmptyServersPayload()
        {
            var payload = string.Empty;

            var converter = new ComputeServerPayloadConverter();
            var containers = converter.ConvertServers(payload).ToList();

            Assert.AreEqual(0, containers.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseANullServersPayload()
        {
            var converter = new ComputeServerPayloadConverter();
            converter.ConvertServers(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidServersJsonPayload()
        {
            var converter = new ComputeServerPayloadConverter();
            converter.ConvertServers("[ { \"SomeAtrib\" }]");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseInvalidServersPayload()
        {
            var converter = new ComputeServerPayloadConverter();
            converter.ConvertServers("NOT JSON");
        }
    }
}
