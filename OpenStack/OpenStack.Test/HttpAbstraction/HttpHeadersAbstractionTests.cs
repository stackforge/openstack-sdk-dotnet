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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.Http;

namespace OpenStack.Test.HttpAbstraction
{
    [TestClass]
    public class HttpHeadersAbstractionTests
    {
        [TestMethod]
        public void CanAddHeaders()
        {
            var headers = new HttpHeadersAbstraction();
            headers.Add("Test","Value");
            Assert.IsTrue(headers.Contains("Test"));
            Assert.AreEqual("Value",headers["Test"].First());
        }

        [TestMethod]
        public void CanAddRangeHeaders()
        {
            var headers = new HttpHeadersAbstraction();

            var rspMsg = new HttpResponseMessage();
            rspMsg.Headers.Add("Test", "Value");

            headers.AddRange(rspMsg.Headers);
            Assert.IsTrue(headers.Contains("Test"));
            Assert.AreEqual("Value", headers["Test"].First());
        }

        [TestMethod]
        public void CanClearHeaders()
        {
            var headers = new HttpHeadersAbstraction();
            headers.Add("Test", "Value");
            Assert.IsTrue(headers.Contains("Test"));

            headers.Clear();
            Assert.AreEqual(0,headers.Count());
        }

        [TestMethod]
        public void CanGetValuesHeaders()
        {
            var headers = new HttpHeadersAbstraction();
            var values = new List<string>() {"value1", "value2"};
            headers.Add("Test", values);
            Assert.IsTrue(headers.Contains("Test"));
            Assert.AreEqual(values, headers.GetValues("Test"));
        }

        [TestMethod]
        public void CanTryGetValuesHeaders()
        {
            var headers = new HttpHeadersAbstraction();
            var values = new List<string>() { "value1", "value2" };
            headers.Add("Test", values);

            IEnumerable<string> resValues = new List<string>();
            Assert.IsTrue(headers.TryGetValue("Test", out resValues));

            Assert.AreEqual(2,resValues.Count());
            Assert.AreEqual(values, resValues);
        }
    }
}
