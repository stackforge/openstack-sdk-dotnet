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
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.Http;

namespace OpenStack.Test.HttpAbstraction
{
    [TestClass]
    public class HttpAbstractionClientTests
    {
        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void CanMakeGetRequest()
        {
            using (var client = new HttpAbstractionClientFactory().Create())
            {
                client.Uri = new Uri("http://httpbin.org/get");
                client.Method = HttpMethod.Get;

                var responseTask = client.SendAsync();
                responseTask.Wait();
                var response = responseTask.Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                Assert.IsNotNull(response.Content);
                var content = TestHelper.GetStringFromStream(response.Content);
                Assert.AreNotEqual(0, content.Length);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void ResponsesIncludeContentHeaders()
        {
            using (var client = new HttpAbstractionClientFactory().Create(new TimeSpan(0, 5, 0), CancellationToken.None))
            {
                client.Uri = new Uri("http://httpbin.org/get");
                client.Method = HttpMethod.Get;

                var responseTask = client.SendAsync();
                responseTask.Wait();
                var response = responseTask.Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                Assert.IsTrue(response.Headers.Contains("Content-Length"));
                Assert.IsTrue(response.Headers.Contains("Content-Type"));

                Assert.IsTrue(int.Parse(response.Headers["Content-Length"].First()) > 200);
                Assert.AreEqual("application/json", response.Headers["Content-Type"].First());

                Assert.IsNotNull(response.Content);
                var content = TestHelper.GetStringFromStream(response.Content);
                Assert.AreNotEqual(0, content.Length);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void CanMakePutRequest()
        {
            using (var client = new HttpAbstractionClientFactory().Create())
            {
                using (var content = TestHelper.CreateStream("Test Text"))
                {
                    client.Uri = new Uri("http://httpbin.org/put");
                    client.Method = HttpMethod.Put;
                    client.Content = content;

                    var responseTask = client.SendAsync();
                    responseTask.Wait();
                    var response = responseTask.Result;

                    Assert.IsNotNull(response);
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                    
                    Assert.IsNotNull(response.Content);

                    var stringContent = TestHelper.GetStringFromStream(response.Content);
                    Assert.AreNotEqual(0, stringContent.Length);
                    Assert.IsTrue(stringContent.Contains("\"data\": \"Test Text\""));
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void CanMakePostRequest()
        {
            using (var client = new HttpAbstractionClientFactory().Create())
            {
                using (var content = TestHelper.CreateStream("Test Text"))
                {
                    client.Uri = new Uri("http://httpbin.org/post");
                    client.Method = HttpMethod.Post;
                    client.Content = content;

                    var responseTask = client.SendAsync();
                    responseTask.Wait();
                    var response = responseTask.Result;

                    Assert.IsNotNull(response);
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                    var stringContent = TestHelper.GetStringFromStream(response.Content);

                    Assert.IsTrue(stringContent.Contains("\"data\": \"Test Text\""));
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void CanMakePostRequestWithNullContent()
        {
            using (var client = new HttpAbstractionClientFactory().Create())
            {
                client.Uri = new Uri("http://httpbin.org/post");
                client.Method = HttpMethod.Post;

                var responseTask = client.SendAsync();
                responseTask.Wait();
                var response = responseTask.Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                var stringContent = TestHelper.GetStringFromStream(response.Content);

                Assert.IsTrue(stringContent.Contains("\"data\": \"\""));

            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void CanMakeDeleteRequest()
        {
            using (var client = new HttpAbstractionClientFactory().Create())
            {

                client.Uri = new Uri("http://httpbin.org/delete");
                client.Method = HttpMethod.Delete;

                var responseTask = client.SendAsync();
                responseTask.Wait();
                var response = responseTask.Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                
                Assert.IsNotNull(response.Content);
                var content = TestHelper.GetStringFromStream(response.Content);
                Assert.AreNotEqual(0, content.Length);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void CanMakeMultipleGetRequestsWithSameClient()
        {
            using (var client = new HttpAbstractionClientFactory().Create())
            {

                client.Uri = new Uri("http://httpbin.org/get");
                client.Method = HttpMethod.Get;

                var responseTask = client.SendAsync();
                responseTask.Wait();

                responseTask = client.SendAsync();
                responseTask.Wait();
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void RequestsHonorsClientSideTimeout()
        {
             try
             {
                 using (var client = new HttpAbstractionClientFactory().Create(TimeSpan.FromMilliseconds(100)))
                 {
                     client.Uri = new Uri("http://httpbin.org/delay/30000");

                     var responseTask = client.SendAsync();
                     responseTask.Wait();
                 }
             }
             catch (AggregateException ex)
             {
                 var inner = ex.InnerException;
                 Assert.IsInstanceOfType(inner,typeof(TimeoutException));
                 Assert.IsTrue(inner.Message.Contains("failed to complete in the given timeout period"));
             }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void RequestsHonorsCancelationToken()
        {
            var token = new CancellationToken(true);
            var startTime = DateTime.Now;

            try
            {
                using (var client = new HttpAbstractionClientFactory().Create(token))
                {
                    client.Uri = new Uri("http://httpbin.org/delay/30000");
                    client.Timeout = TimeSpan.FromSeconds(31);

                    var responseTask = client.SendAsync();
                    responseTask.Wait();
                }
            }
            catch (AggregateException ex)
            {
                var inner = ex.InnerException;
               Assert.IsTrue(DateTime.Now - startTime < TimeSpan.FromSeconds(30));
               Assert.IsTrue(inner.Message.Contains("was canceled"));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void RequestsHandlesHttpErrors()
        {
            using (var client = new HttpAbstractionClientFactory().Create())
            {
                client.Uri = new Uri("http://httpbin.org/status/404");

                var responseTask = client.SendAsync();
                responseTask.Wait();
                var response = responseTask.Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

                var stringContent = TestHelper.GetStringFromStream(response.Content);

                Assert.AreEqual(string.Empty, stringContent);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("LongRunning")]
        public void RequestsCanSendHeaders()
        {
            using (var client = new HttpAbstractionClientFactory().Create())
            {
                client.Uri = new Uri("http://httpbin.org/get");
                client.Headers.Add("X-Test-Header","TEST");

                //Added in order to force httpbin not to cache the responses from any prior get requests/unit tests. 
                client.Headers.Add("Cache-Control", "max-age=0");

                var responseTask = client.SendAsync();
                
                responseTask.Wait();
                var response = responseTask.Result;
                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                Assert.IsNotNull(response.Content);
                var content = TestHelper.GetStringFromStream(response.Content);
                Assert.AreNotEqual(0, content.Length);

                Assert.IsTrue(content.Contains("\"X-Test-Header\": \"TEST\""));
            }
        }
    }
}
