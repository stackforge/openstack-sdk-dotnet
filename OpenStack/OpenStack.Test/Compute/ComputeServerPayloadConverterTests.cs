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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    [TestClass]
    public class ComputeServerPayloadConverterTests
    {
        internal string CreateServerJsonFixtrue(string name, string id, string ram, string disk, string vcpus,
            string permaUri, string publicUri)
        {
            var ComputeServerJsonResponseFixture = @"{{
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

            return string.Format(ComputeServerJsonResponseFixture, name, publicUri, permaUri, ram, vcpus, disk, id);
        }

        internal string CreateServerSummaryJsonFixtrue(string id, string permaUri, string publicUri, string adminPass)
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

        [TestMethod]
        public void CanConvertSummaryJsonPayloadToServer()
        {
            
            var serverId = "1";
            var serverPublicUri = "http://www.server.com/v2/servers/1";
            var serverPermUri = "http://www.server.com/servers/1";
            var serverAdminPass = "ABCDE";

            var computeServerFixture = CreateServerSummaryJsonFixtrue(serverId, serverPermUri, serverPublicUri, serverAdminPass);

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
    }
}
