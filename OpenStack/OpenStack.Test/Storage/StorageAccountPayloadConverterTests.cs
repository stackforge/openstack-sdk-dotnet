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
    public class StorageAccountPayloadConverterTests
    {
        [TestMethod]
        public void CanParseAccountWithValidJsonPayloadAndHeaders()
        {
            var accountName = "1234567890";
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            var account = converter.Convert(accountName, headers, validSingleContainerJson);

            Assert.IsNotNull(account);
            Assert.AreEqual(accountName, account.Name);
            Assert.AreEqual(12345, account.TotalBytesUsed);
            Assert.AreEqual(1, account.TotalObjectCount);
            Assert.AreEqual(1, account.TotalContainerCount);
            Assert.AreEqual(1, account.Containers.ToList().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseAccountWithMissingBytesUsedHeader()
        {
            var accountName = "1234567890";
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, headers, validSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseAccountWithMissingObjectCountHeader()
        {
            var accountName = "1234567890";
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Container-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, headers, validSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseAccountWithMissingContainerCountHeader()
        {
            var accountName = "1234567890";
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Object-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, headers, validSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseAccountWithBadBytesUsedHeader()
        {
            var accountName = "1234567890";
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "NOT A NUMBER"},
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, headers, validSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseAccountWithBadObjectCountHeader()
        {
            var accountName = "1234567890";
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Object-Count", "NOT A NUMBER"},
                {"X-Account-Container-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, headers, validSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseAccountWithBadContainerCountHeader()
        {
            var accountName = "1234567890";
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "NOT A NUMBER"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, headers, validSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CannotParseAccountWithBadPayload()
        {
            var accountName = "1234567890";
            var invalidSingleContainerJson = @"[
                                            {
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, headers, invalidSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseAccountWithNullName()
        {
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(null, headers, validSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseAccountWithNullHeaders()
        {
            var accountName = "1234567890";
            var validSingleContainerJson = @"[
                                            {
                                                  ""count"": 1,
                                                  ""bytes"": 7,
                                                  ""name"": ""TestContainer""
                                            }]";

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, null, validSingleContainerJson);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotParseAccountWithNullPayload()
        {
            var accountName = "1234567890";

            var headers = new HttpHeadersAbstraction
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Object-Count", "1"},
                {"X-Account-Container-Count", "1"}
            };

            var converter = new StorageAccountPayloadConverter(new ServiceLocator());
            converter.Convert(accountName, headers, null);
        }
    }
}
