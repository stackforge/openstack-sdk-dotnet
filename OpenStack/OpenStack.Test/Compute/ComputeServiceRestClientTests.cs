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
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;
using OpenStack.Identity;

namespace OpenStack.Test.Compute
{
    [TestClass]
    public class ComputeServiceRestClientTests
    {
        internal ComputeRestSimulator simulator;
        internal string authId = "12345";
        internal Uri endpoint = new Uri("http://testcomputeendpoint.com/v2/1234567890");
        internal IServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.simulator = new ComputeRestSimulator();
            this.ServiceLocator = new ServiceLocator();

            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IHttpAbstractionClientFactory), new ComputeRestSimulatorFactory(simulator));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.simulator = new ComputeRestSimulator();
            this.ServiceLocator = new ServiceLocator();
        }

        ServiceClientContext GetValidContext()
        {
            return GetValidContext(CancellationToken.None);
        }

        ServiceClientContext GetValidContext(CancellationToken token)
        {
            var creds = new OpenStackCredential(this.endpoint, "SomeUser", "Password", "SomeTenant", "region-a.geo-1");
            creds.SetAccessTokenId(this.authId);

            return new ServiceClientContext(creds, token, "Nova", endpoint);
        }

        #region Get Flavors Test

        [TestMethod]
        public async Task GetComputeFlavorsIncludesAuthHeader()
        {
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFlavors();

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetComputeFlavorsFormsCorrectUrlAndMethod()
        {
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFlavors();

            Assert.AreEqual(string.Format("{0}/flavors", endpoint), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanGetFlavors()
        {
            this.simulator.Flavors.Add(new ComputeFlavor("1", "tiny", "4", "2", "10", new Uri("http://testcomputeendpoint.com/v2/1234567890/flavors/1"), new Uri("http://testcomputeendpoint.com/1234567890/flavors/1")));

            var client =
                 new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetFlavors();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);
        }

        #endregion

        #region Get Flavor Test

        [TestMethod]
        public async Task GetComputeFlavorIncludesAuthHeader()
        {
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFlavor("1");

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetComputeFlavorFormsCorrectUrlAndMethod()
        {
            var flavorId = "1";
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetFlavor(flavorId);

            Assert.AreEqual(string.Format("{0}/flavors/{1}", endpoint, flavorId), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanGetFlavor()
        {
            var flavorId = "1";
            this.simulator.Flavors.Add(new ComputeFlavor(flavorId, "tiny", "4", "2", "10", new Uri("http://testcomputeendpoint.com/v2/1234567890/flavors/1"), new Uri("http://testcomputeendpoint.com/1234567890/flavors/1")));

            var client =
                 new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetFlavor(flavorId);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);
        }

        #endregion

        #region Get Images Test

        [TestMethod]
        public async Task GetComputeImagesIncludesAuthHeader()
        {
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetImages();

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetComputeImagesFormsCorrectUrlAndMethod()
        {
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetImages();

            Assert.AreEqual(string.Format("{0}/images/detail", endpoint), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanGetImages()
        {
            var created = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10));
            var updated = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            this.simulator.Images.Add(new ComputeImage("1", "tiny",
                new Uri("http://testcomputeendpoint.com/v2/1234567890/flavors/1"),
                new Uri("http://testcomputeendpoint.com/1234567890/flavors/1"), "ACTIVE", created, updated, 10, 512, 100));

            var client =
                 new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetImages();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);
        }

        #endregion

        #region Get Image Test

        [TestMethod]
        public async Task GetComputeImageIncludesAuthHeader()
        {
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetImage("12345");

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task GetComputeImageFormsCorrectUrlAndMethod()
        {
            var imageId = "12345";
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.GetImage(imageId);

            Assert.AreEqual(string.Format("{0}/images/{1}", endpoint, imageId), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Get, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanGetImage()
        {
            var imageId = "12345";
            var created = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10));
            var updated = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            this.simulator.Images.Add(new ComputeImage(imageId, "tiny",
                new Uri("http://testcomputeendpoint.com/v2/1234567890/flavors/1"),
                new Uri("http://testcomputeendpoint.com/1234567890/flavors/1"), "ACTIVE", created, updated, 10, 512, 100));

            var client =
                 new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.GetImage(imageId);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var respContent = TestHelper.GetStringFromStream(resp.Content);
            Assert.IsTrue(respContent.Length > 0);
        }

        #endregion

        #region Delete Image Test

        [TestMethod]
        public async Task DeleteComputeImageIncludesAuthHeader()
        {
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.DeleteImage("12345");

            Assert.IsTrue(this.simulator.Headers.ContainsKey("X-Auth-Token"));
            Assert.AreEqual(this.authId, this.simulator.Headers["X-Auth-Token"]);
        }

        [TestMethod]
        public async Task DeleteComputeImageFormsCorrectUrlAndMethod()
        {
            var imageId = "12345";
            var client =
                new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            await client.DeleteImage(imageId);

            Assert.AreEqual(string.Format("{0}/images/{1}", endpoint, imageId), this.simulator.Uri.ToString());
            Assert.AreEqual(HttpMethod.Delete, this.simulator.Method);
        }

        [TestMethod]
        public async Task CanDeleteImage()
        {
            var imageId = "12345";
            var created = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10));
            var updated = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            this.simulator.Images.Add(new ComputeImage(imageId, "tiny",
                new Uri("http://testcomputeendpoint.com/v2/1234567890/flavors/1"),
                new Uri("http://testcomputeendpoint.com/1234567890/flavors/1"), "ACTIVE", created, updated, 10, 512, 100));

            var client =
                 new ComputeServiceRestClient(GetValidContext(), this.ServiceLocator);

            var resp = await client.DeleteImage(imageId);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            Assert.IsFalse(this.simulator.Images.Any());
        }

        #endregion
    }
}
