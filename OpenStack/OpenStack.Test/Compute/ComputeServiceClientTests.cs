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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;
using OpenStack.Identity;

namespace OpenStack.Test.Compute
{
    [TestClass]
    public class ComputeServiceClientTests
    {
        internal TestComputeServicePocoClient ServicePocoClient;
        
        internal string authId = "12345";
        internal string endpoint = "http://testcomputeendpoint.com/v2/1234567890";
        internal ServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.ServicePocoClient = new TestComputeServicePocoClient();
            this.ServiceLocator = new ServiceLocator();
            
            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IComputeServicePocoClientFactory), new TestComputeServicePocoClientFactory(this.ServicePocoClient));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.ServicePocoClient = new TestComputeServicePocoClient();
            this.ServiceLocator = new ServiceLocator();
        }

        IOpenStackCredential GetValidCreds()
        {
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Nova", "Compute Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(endpoint, string.Empty, "some version", "some version info", "1,2,3")
                }));

            var creds = new OpenStackCredential(new Uri(this.endpoint), "SomeUser", "Password", "SomeTenant");
            creds.SetAccessTokenId(this.authId);
            creds.SetServiceCatalog(catalog);
            return creds;
        }

        [TestMethod]
        public async Task CanGetFlavors()
        {
            var flv1 = new ComputeFlavor("1", "m1.tiny", "512", "2", "10", new Uri("http://someuri.com/v2/flavors/1"),
                new Uri("http://someuri.com/flavors/1"), new Dictionary<string, string>());
            var flv2 = new ComputeFlavor("2", "m1.small", "1024", "4", "100", new Uri("http://someuri.com/v2/flavors/2"),
                new Uri("http://someuri.com/flavors/2"), new Dictionary<string, string>());
            var flavors = new List<ComputeFlavor>() {flv1, flv2};

            this.ServicePocoClient.GetFlavorsDelegate = () => Task.Factory.StartNew(() => (IEnumerable<ComputeFlavor>)flavors);

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetFlavors();
            Assert.IsNotNull(resp);

            var respFlavors = resp.ToList();
            Assert.AreEqual(2, respFlavors.Count());
            Assert.AreEqual(flv1, respFlavors[0]);
            Assert.AreEqual(flv2, respFlavors[1]);
        }

        [TestMethod]
        public async Task CanGetFlavor()
        {
            var expectedFlavor = new ComputeFlavor("1", "m1.tiny", "512", "2", "10", new Uri("http://someuri.com/v2/flavors/1"),
                new Uri("http://someuri.com/flavors/1"), new Dictionary<string, string>());

            this.ServicePocoClient.GetFlavorDelegate = (id) =>
            {
                Assert.AreEqual("1", id);
                return Task.Factory.StartNew(() => expectedFlavor);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var flavor = await client.GetFlavor("1");
            
            Assert.IsNotNull(flavor);
            Assert.AreEqual(expectedFlavor, flavor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetFlavorWithNullFlavorIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetFlavor(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetFlavorWithEmptyFlavorIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetFlavor(string.Empty);
        }

        [TestMethod]
        public async Task CanCreateServer()
        {
            var serverName = "MyServer";
            var imageId = "56789";
            var keyName = "MyKey";
            var flavorId = "2";
            var networkId = "98765";
            var adminPassword = "ABCDE";
            var expServer = new ComputeServer("1235", serverName, adminPassword, new Uri("http://someuri.com/v2/servers/12345"),
                new Uri("http://someuri.com/servers/12345"), new Dictionary<string, string>());

            this.ServicePocoClient.CreateServerDelegate = (name, imgId, flvId, ntwId, key, groups) =>
            {
                Assert.AreEqual(serverName, name);
                Assert.AreEqual(imageId, imgId);
                Assert.AreEqual(flavorId, flvId);
                Assert.AreEqual(networkId, ntwId);
                Assert.AreEqual(keyName, key);
                Assert.IsTrue(groups.Any(g => g == "default"));
                return Task.Factory.StartNew(() => expServer);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var server = await client.CreateServer(serverName, imageId, flavorId, networkId, keyName, new List<string>() { "default" });

            Assert.IsNotNull(server);
            Assert.AreEqual("1235", server.Id);
            Assert.AreEqual(adminPassword, server.AdminPassword);
        }

        [TestMethod]
        public async Task CanCreateServerWithoutKeyName()
        {
            var serverName = "MyServer";
            var imageId = "56789";
            var flavorId = "2";
            var networkId = "98765";
            var adminPassword = "ABCDE";
            var expServer = new ComputeServer("1235", serverName, adminPassword, new Uri("http://someuri.com/v2/servers/12345"),
                new Uri("http://someuri.com/servers/12345"), new Dictionary<string, string>());

            this.ServicePocoClient.CreateServerDelegate = (name, imgId, flvId, ntwId, key, groups) =>
            {
                Assert.AreEqual(serverName, name);
                Assert.AreEqual(imageId, imgId);
                Assert.AreEqual(flavorId, flvId);
                Assert.AreEqual(networkId, ntwId);
                Assert.AreEqual(string.Empty, key);
                Assert.IsTrue(groups.Any(g => g == "default"));
                return Task.Factory.StartNew(() => expServer);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var server = await client.CreateServer(serverName, imageId, flavorId, networkId, new List<string>() { "default" });

            Assert.IsNotNull(server);
            Assert.AreEqual("1235", server.Id);
            Assert.AreEqual(adminPassword, server.AdminPassword);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateServerWithNullNameThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.CreateServer(null, "12345", "2", "54321", new List<string>() { "default" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateServerWithEmptyNameThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.CreateServer(string.Empty, "12345", "2", "54321", new List<string>() { "default" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateServerWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.CreateServer("MyServer", null, "2", "54321", new List<string>() { "default" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateServerWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.CreateServer("MyServer", string.Empty, "2", "54321", new List<string>() { "default" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateServerWithNullFlavorIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.CreateServer("MyServer", "12345", null, "54321", new List<string>() { "default" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateServerWithEmptyFlavorIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.CreateServer("MyServer", "12345", string.Empty, "54321", new List<string>() { "default" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateServerWithNullNetworkIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.CreateServer("MyServer", "12345", "2", null, new List<string>() { "default" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateServerWithEmptyNetworkIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.CreateServer("MyServer", "12345", "2", string.Empty, new List<string>() { "default" });
        }

        [TestMethod]
        public async Task CanGetServers()
        {
            var expServer1 = new ComputeServer("1", "srv1",
                new Uri("http://testcomputeendpoint.com/v2/1234567890/servers/1"),
                new Uri("http://testcomputeendpoint.com/1234567890/servers/1"), new Dictionary<string, string>());

            var expServer2 = new ComputeServer("2", "srv2",
                new Uri("http://testcomputeendpoint.com/v2/1234567890/servers/1"),
                new Uri("http://testcomputeendpoint.com/1234567890/servers/1"), new Dictionary<string, string>());
            var servers = new List<ComputeServer>() { expServer1, expServer2 };

            this.ServicePocoClient.GetServersDelegate = () => Task.Factory.StartNew(() => (IEnumerable<ComputeServer>)servers);

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetServers();
            Assert.IsNotNull(resp);

            var respFlavors = resp.ToList();
            Assert.AreEqual(2, respFlavors.Count());
            Assert.AreEqual(expServer1, respFlavors[0]);
            Assert.AreEqual(expServer2, respFlavors[1]);
        }

        [TestMethod]
        public async Task CanGetServer()
        {
            var serverId = "12345";
            var expServer = new ComputeServer(serverId, "tiny",
                new Uri("http://testcomputeendpoint.com/v2/1234567890/servers/1"),
                new Uri("http://testcomputeendpoint.com/1234567890/servers/1"), new Dictionary<string, string>());

            this.ServicePocoClient.GetServerDelegate = (id) =>
            {
                Assert.AreEqual(serverId, id);
                return Task.Factory.StartNew(() => expServer);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var server = await client.GetServer(serverId);

            Assert.IsNotNull(server);
            Assert.AreEqual(expServer, server);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetServerWithNullIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetServer(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetServerWithEmptyIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetServer(string.Empty);
        }

        [TestMethod]
        public async Task CanDeleteServer()
        {
            this.ServicePocoClient.DeleteServerDelegate = async (serverId) =>
            {
                await Task.Run(() => Assert.AreEqual(serverId, "12345"));
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteServer("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteServerWithNullServerIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteServer(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteServerWithEmptyServerIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteServer(string.Empty);
        }

        [TestMethod]
        public async Task CanAssignFloatingIp()
        {
            this.ServicePocoClient.AssignFloatingIpDelegate = async (serverId, ipAddress) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(serverId, "12345");
                    Assert.AreEqual(ipAddress, "172.0.0.1");
                });
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.AssignFloatingIp("12345", "172.0.0.1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AssignFloatingIpWithNullServerIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.AssignFloatingIp(null, "172.0.0.1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AssignFloatingIpWithEmptyServerIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.AssignFloatingIp(string.Empty, "172.0.0.1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AssignFloatingIpWithNullIpAddressThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.AssignFloatingIp("12345", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AssignFloatingIpWithEmptyIpAddressThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.AssignFloatingIp("12345", string.Empty);
        }

        [TestMethod]
        public async Task CanGetServerMetadata()
        {
            var meta = new Dictionary<string, string>() { { "item1", "value1" } };

            this.ServicePocoClient.GetServerMetadataDelegate = (id) =>
            {
                Assert.AreEqual("12345", id);
                return Task.Factory.StartNew(() => (IDictionary<string, string>)meta);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var metadata = await client.GetServerMetadata("12345");

            Assert.IsNotNull(metadata);
            Assert.AreEqual(meta, metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetServerMetadataWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetServerMetadata(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetServerMetadataWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetServerMetadata(string.Empty);
        }

        [TestMethod]
        public async Task CanDeleteServerMetadata()
        {
            this.ServicePocoClient.DeleteServerMetadataDelegate = async (flavorId, key) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(flavorId, "12345");
                    Assert.AreEqual(key, "item1");
                });
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteServerMetadata("12345", "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteServerMetadataWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteServerMetadata(null, "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteServerMetadataWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteServerMetadata(string.Empty, "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteServerMetadataWithEmptyKeyThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteServerMetadata("12345", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteServerMetadataWithNullKeyThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteServerMetadata("12345", null);
        }

        [TestMethod]
        public async Task CanUpdateServerMetadata()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" } };
            this.ServicePocoClient.UpdateServerMetadataDelegate = async (flavorId, meta) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(flavorId, "12345");
                    Assert.AreEqual(metadata, meta);
                });
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.UpdateServerMetadata("12345", metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateServerMetadataWithNullImageIdThrows()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" } };
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.UpdateServerMetadata(null, metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateServerMetadataWithEmptyImageIdThrows()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" } };
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.UpdateServerMetadata(string.Empty, metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateServerMetadataWithNullMetadataThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.UpdateServerMetadata("12345", null);
        }

        [TestMethod]
        public async Task CanGetImages()
        {
            var img1 = new ComputeImage("12345", "image1", new Uri("http://someuri.com/v2/images/12345"), new Uri("http://someuri.com/images/12345"), new Dictionary<string, string>(), "active", DateTime.Now, DateTime.Now, 10, 512, 100);
            var img2 = new ComputeImage("23456", "image2", new Uri("http://someuri.com/v2/images/23456"), new Uri("http://someuri.com/images/23456"), new Dictionary<string, string>(), "active", DateTime.Now, DateTime.Now, 10, 512, 100);
            var images = new List<ComputeImage>() { img1, img2 };

            this.ServicePocoClient.GetImagesDelegate = () => Task.Factory.StartNew(() => (IEnumerable<ComputeImage>)images);

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetImages();
            Assert.IsNotNull(resp);

            var respImage = resp.ToList();
            Assert.AreEqual(2, respImage.Count());
            Assert.AreEqual(img1, respImage[0]);
            Assert.AreEqual(img2, respImage[1]);
        }

        [TestMethod]
        public async Task CanGetImage()
        {
            var img1 = new ComputeImage("12345", "image1", new Uri("http://someuri.com/v2/images/12345"), new Uri("http://someuri.com/images/12345"), new Dictionary<string, string>(), "active", DateTime.Now, DateTime.Now, 10, 512, 100);

            this.ServicePocoClient.GetImageDelegate = (id) =>
            {
                Assert.AreEqual("12345", id);
                return Task.Factory.StartNew(() => img1);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var image = await client.GetImage("12345");

            Assert.IsNotNull(image);
            Assert.AreEqual(img1, image);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetImageWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetImage(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetImageWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetImage(string.Empty);
        }

        [TestMethod]
        public async Task CanDeleteImage()
        {
            this.ServicePocoClient.DeleteImageDelegate = async (imageId) =>
            {
                await Task.Run(() => Assert.AreEqual(imageId, "12345"));
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImage("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteImageWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImage(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteImageWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImage(string.Empty);
        }

        [TestMethod]
        public async Task CanGetImageMetadata()
        {
            var meta = new Dictionary<string, string>() { { "item1", "value1" } };

            this.ServicePocoClient.GetImageMetadataDelegate = (id) =>
            {
                Assert.AreEqual("12345", id);
                return Task.Factory.StartNew(() => (IDictionary<string, string>)meta);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var metadata = await client.GetImageMetadata("12345");

            Assert.IsNotNull(metadata);
            Assert.AreEqual(meta, metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetImageMetadataWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetImageMetadata(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetImageMetadataWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetImageMetadata(string.Empty);
        }

        [TestMethod]
        public async Task CanDeleteImageMetadata()
        {
            this.ServicePocoClient.DeleteImageMetadataDelegate = async (imageId, key) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(imageId, "12345");
                    Assert.AreEqual(key, "item1");
                });
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImageMetadata("12345", "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteImageMetadataWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImageMetadata(null, "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteImageMetadataWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImageMetadata(string.Empty, "item1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteImageMetadataWithEmptyKeyThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImageMetadata("12345", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteImageMetadataWithNullKeyThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImageMetadata("12345", null);
        }

        [TestMethod]
        public async Task CanUpdateImageMetadata()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" } };
            this.ServicePocoClient.UpdateImageMetadataDelegate = async (imageId, meta) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(imageId, "12345");
                    Assert.AreEqual(metadata, meta);
                });
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.UpdateImageMetadata("12345", metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateImageMetadataWithNullImageIdThrows()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" } };
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.UpdateImageMetadata(null, metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateImageMetadataWithEmptyImageIdThrows()
        {
            var metadata = new Dictionary<string, string>() { { "item1", "value1" } };
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.UpdateImageMetadata(string.Empty, metadata);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateImageMetadataWithNullMetadataThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.UpdateImageMetadata("12345", null);
        }

        [TestMethod]
        public async Task CanGetKeyPairs()
        {
            var expKeyPair1 = new ComputeKeyPair("1", "ABCDEF","12345");
            var expKeyPair2 = new ComputeKeyPair("2", "FEDCBA", "54321");
            var pairs = new List<ComputeKeyPair>() { expKeyPair1, expKeyPair2 };

            this.ServicePocoClient.GetKeyPairsDelegate = () => Task.Factory.StartNew(() => (IEnumerable<ComputeKeyPair>)pairs);

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var resp = await client.GetKeyPairs();
            Assert.IsNotNull(resp);

            var respPairs = resp.ToList();
            Assert.AreEqual(2, respPairs.Count());
            Assert.AreEqual(expKeyPair1, respPairs[0]);
            Assert.AreEqual(expKeyPair2, respPairs[1]);
        }

        [TestMethod]
        public async Task CanGetKeyPair()
        {
            var keyName = "1";
            var expKeyPair1 = new ComputeKeyPair(keyName, "ABCDEF", "12345");

            this.ServicePocoClient.GetKeyPairDelegate = (name) =>
            {
                Assert.AreEqual(keyName, name);
                return Task.Factory.StartNew(() => expKeyPair1);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var keyPair = await client.GetKeyPair(keyName);

            Assert.IsNotNull(keyPair);
            Assert.AreEqual(expKeyPair1, keyPair);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetKeyPairWithNullNameThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetKeyPair(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetKeyPairWithEmptyNameThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetKeyPair(string.Empty);
        }
    }
}
