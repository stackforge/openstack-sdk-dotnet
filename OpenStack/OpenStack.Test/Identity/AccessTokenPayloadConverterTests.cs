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
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Identity;

namespace OpenStack.Test.Identity
{
    [TestClass]
    public class AccessTokenPayloadConverterTests
    {
        [TestMethod]
        public void CanConvertJsonPayload()
        {
            var AuthJsonResponseFixture = @"{
                                                ""access"": {
                                                    ""token"": {
                                                        ""expires"": ""2014-03-18T10:59:46.355Z"",
                                                        ""id"": ""HPAuth10_af3d1bfe456d18e8d4793e54922f839fa051d9f60f115aca52c9a44f9e3d96fb"",
                                                        ""tenant"": {
                                                            ""id"": ""10244656540440"",
                                                            ""name"": ""10255892528404-Project""
                                                        }
                                                    }
                                                }
                                            }";

            var expectedToken = "HPAuth10_af3d1bfe456d18e8d4793e54922f839fa051d9f60f115aca52c9a44f9e3d96fb"; 
            var converter = new AccessTokenPayloadConverter();
            var token = converter.Convert(AuthJsonResponseFixture);
            Assert.IsNotNull(token);
            Assert.AreEqual(expectedToken, token);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingTokenId()
        {
            var missingTokenIdFixture = @"{
                                                ""access"": {
                                                    ""token"": {
                                                        ""expires"": ""2014-03-18T10:59:46.355Z"",
                                                       ""tenant"": {
                                                            ""id"": ""10244656540440"",
                                                            ""name"": ""10255892528404-Project""
                                                        }
                                                    }
                                                }
                                            }";

            var converter = new AccessTokenPayloadConverter();
            converter.Convert(missingTokenIdFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadMissingToken()
        {
            var missingTokenFixture = @"{
                                          ""access"": { }
                                        }";

            var converter = new AccessTokenPayloadConverter();
            converter.Convert(missingTokenFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertJsonPayloadEmptyObject()
        {
            var emptyObjectFixture = @"{ }";

            var converter = new AccessTokenPayloadConverter();
            converter.Convert(emptyObjectFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertInvalidJson()
        {
            var badJsonFixture = @"{ NOT JSON";

            var converter = new AccessTokenPayloadConverter();
            converter.Convert(badJsonFixture);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotConvertNonObjectJson()
        {
            var nonObjectJson = @"[]";

            var converter = new AccessTokenPayloadConverter();
            converter.Convert(nonObjectJson);
        }
    }
}
