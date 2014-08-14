// /* ============================================================================
// Copyright 2014 Hewlett Packard
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    public class ComputeRestSimulator : RestSimulator
    {
        internal ICollection<ComputeFlavor> Flavors { get; private set; }

        internal ICollection<ComputeImage> Images { get; private set; }

        internal ICollection<ComputeServer> Servers { get; private set; }

        internal ICollection<ComputeKeyPair> KeyPairs { get; private set; }

        public ComputeRestSimulator() : base()
        {
            this.Flavors = new List<ComputeFlavor>();
            this.Images = new List<ComputeImage>();
            this.Servers = new List<ComputeServer>();
            this.KeyPairs = new List<ComputeKeyPair>();
        }

        public ComputeRestSimulator(CancellationToken token) : this()
        {
        }

        protected override IHttpResponseAbstraction HandleGet()
        {
            if (this.Uri.Segments.Count() >= 4)
            {
                switch (this.Uri.Segments[3].TrimEnd('/').ToLower())
                {
                    case "flavors":
                        return HandleGetFlavor();
                    case "images":
                        return HandleGetImage();
                    case "servers":
                        return HandleGetServer();
                    case "os-keypairs":
                        return HandleGetKeyPairs();
                }
            }
            throw new NotImplementedException();
        }

        internal IHttpResponseAbstraction HandleGetFlavor()
        {
            Stream flavorContent;
            switch (this.Uri.Segments.Count())
            {
                case 5:
                    //flavor id given, list single flavor
                    var flavorId = this.Uri.Segments[4].TrimEnd('/');
                    var flavor =
                        this.Flavors.FirstOrDefault(
                            f => string.Equals(f.Id, flavorId, StringComparison.OrdinalIgnoreCase));
                    if (flavor == null)
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }

                    flavorContent = TestHelper.CreateStream(GenerateFlavorPayload(flavor));
                    break;
                case 4:
                    //no flavor id given, list all flavors
                    flavorContent = TestHelper.CreateStream(GenerateItemsPayload(this.Flavors, "flavors"));
                    break;
                default:
                    //Unknown Uri format
                    throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), flavorContent);
        }

        internal IHttpResponseAbstraction HandleGetKeyPairs()
        {
            Stream keyPairContent;
            switch (this.Uri.Segments.Count())
            {
                case 5:
                    var keyPairName = this.Uri.Segments[4].TrimEnd('/');
                    var pair =
                        this.KeyPairs.FirstOrDefault(
                            kp => string.Equals(kp.Name, keyPairName, StringComparison.OrdinalIgnoreCase));
                    if (pair == null)
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }

                    keyPairContent = TestHelper.CreateStream(GenerateKeyPairPayload(pair));
                    break;
                case 4:
                    keyPairContent = TestHelper.CreateStream(GenerateKeyPairsPayload(this.KeyPairs));
                    break;
                default:
                    //Unknown Uri format
                    throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), keyPairContent);
        }

        internal IHttpResponseAbstraction HandleGetImage()
        {
            Stream imageContent;
            switch (this.Uri.Segments.Count())
            {
                case 5:
                    //image id given, list single image
                    var imageId = this.Uri.Segments[4].TrimEnd('/');
                    if (imageId.ToLower() == "detail")
                    {
                        imageContent = TestHelper.CreateStream(GenerateItemsPayload(this.Images, "images"));
                    }
                    else
                    {
                        var image =
                            this.Images.FirstOrDefault(
                                f => string.Equals(f.Id, imageId, StringComparison.OrdinalIgnoreCase));
                        if (image == null)
                        {
                            return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                        }

                        imageContent = TestHelper.CreateStream(GenerateImagePayload(image));
                    }
                    break;
                case 6:
                     if (this.Uri.Segments[5].TrimEnd('/').ToLower() != "metadata")
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }
                    var imgId = this.Uri.Segments[4].TrimEnd('/');
                    var img =
                        this.Images.FirstOrDefault(
                            i => string.Equals(i.Id, imgId, StringComparison.OrdinalIgnoreCase));
                    if (img == null)
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }
                    imageContent = TestHelper.CreateStream(GenerateMetadataPayload(img.Metadata));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), imageContent);
        }

        internal IHttpResponseAbstraction HandleGetServer()
        {
            Stream serverContent;

            if (this.Uri.Segments.Count() < 5)
            {
                if (this.Uri.Segments.Count() == 4)
                {
                    serverContent = TestHelper.CreateStream(GenerateItemsPayload(this.Flavors, "servers"));
                    return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), serverContent);
                }
                throw new NotImplementedException();
            }

            var srvId = this.Uri.Segments[4].TrimEnd('/');
            var srv =
                this.Servers.FirstOrDefault(
                    s => string.Equals(s.Id, srvId, StringComparison.OrdinalIgnoreCase));
            if (srv == null)
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            
            switch (this.Uri.Segments.Count())
            {
                case 6:
                    if (this.Uri.Segments[5].TrimEnd('/').ToLower() != "metadata")
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }
                    
                    serverContent = TestHelper.CreateStream(GenerateMetadataPayload(srv.Metadata));
                    break;
                case 5:
                    //server id given, list single server
                    serverContent = TestHelper.CreateStream(GenerateServerPayload(srv));
                    break;
                default:
                    //Unknown Uri format
                    throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), serverContent);
        }

        protected override IHttpResponseAbstraction HandlePost()
        {
            if (this.Uri.Segments.Count() >= 4)
            {
                switch (this.Uri.Segments[3].TrimEnd('/').ToLower())
                {
                    case "servers":
                        return HandlePostServer();
                    case "images":
                        return HandlePostImage();
                }
            }
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandlePut()
        {
            throw new NotImplementedException();
        }

        internal IHttpResponseAbstraction HandlePostImage()
        {
            switch (this.Uri.Segments.Count())
            {
                case 6:
                    if (this.Uri.Segments[5].TrimEnd('/').ToLower() != "metadata")
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }
                    var imgId = this.Uri.Segments[4].TrimEnd('/');
                    var img =
                        this.Images.FirstOrDefault(
                            i => string.Equals(i.Id, imgId, StringComparison.OrdinalIgnoreCase));
                    if (img == null)
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }

                    ParseAndStoreMetadata(img, GetPayload(this.Content));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>());
        }

        internal IHttpResponseAbstraction HandlePostServer()
        {
            switch (this.Uri.Segments.Count())
            {
                case 4:
                    return HandlePostNewServer();
                case 6:
                    var srvId = this.Uri.Segments[4].TrimEnd('/');
                    var action = this.Uri.Segments[5].TrimEnd('/').ToLower();
                    switch (action)
                    {
                        case "metadata":
                            return HandlePostServerMetadata(srvId);
                        case "action":
                            return HandlePostServerAction(srvId);
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        internal IHttpResponseAbstraction HandlePostServerAction(string serverId)
        {
            this.Content.Position = 0;
            var reqBody = TestHelper.GetStringFromStream(this.Content);
            var body = JObject.Parse(reqBody);
            if (body["addFloatingIp"] != null && body["addFloatingIp"]["address"] != null)
            {
                return TestHelper.CreateResponse(HttpStatusCode.Accepted, new Dictionary<string, string>());
            }

            throw new NotImplementedException();
        }

        internal IHttpResponseAbstraction HandlePostServerMetadata(string serverId)
        {
            var srv =
                        this.Servers.FirstOrDefault(
                            s => string.Equals(s.Id, serverId, StringComparison.OrdinalIgnoreCase));
            if (srv == null)
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            ParseAndStoreMetadata(srv, GetPayload(this.Content));
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>());
        }

        internal IHttpResponseAbstraction HandlePostNewServer()
        {
            var payload = TestHelper.GetStringFromStream(this.Content);
            var obj = JObject.Parse(payload);
            var name = (string)obj["server"]["name"];
            var srv = new ComputeServer(Guid.NewGuid().ToString(), name, "12345", new Uri("http://test.com"), new Uri("http://test.com"), new Dictionary<string, string>());
            this.Servers.Add(srv);
            var responsePayload = GenerateCreateServeResponse(srv);

            return TestHelper.CreateResponse(HttpStatusCode.Accepted, new Dictionary<string, string>(), responsePayload.ConvertToStream());
        }

        protected override IHttpResponseAbstraction HandleDelete()
        {
            if (this.Uri.Segments.Count() >= 4)
            {
                switch (this.Uri.Segments[3].TrimEnd('/').ToLower())
                {
                    case "images":
                        return HandleDeleteImages();
                    case "flavors":
                        return HandleDeleteFlavors();
                    case "servers":
                        return HandleDeleteServers();
                }
            }
            return TestHelper.CreateResponse(HttpStatusCode.NotFound);
        }

        private IHttpResponseAbstraction HandleDeleteImages()
        {
            if (this.Uri.Segments.Count() < 5)
            {
                throw new NotImplementedException();
            }

            var imageId = this.Uri.Segments[4].TrimEnd('/');
            var image = this.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            switch (this.Uri.Segments.Count())
            {
                case 5:
                    this.Images.Remove(image);
                    return TestHelper.CreateResponse(HttpStatusCode.OK);
                case 7:
                    var key = this.Uri.Segments[6].TrimEnd('/');
                    if (!image.Metadata.ContainsKey(key))
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound); 
                    }
                    image.Metadata.Remove(key);
                    return TestHelper.CreateResponse(HttpStatusCode.OK);
                default:
                    throw new NotImplementedException();
            }
        }

        private IHttpResponseAbstraction HandleDeleteFlavors()
        {
            throw new NotImplementedException();
        }

        private IHttpResponseAbstraction HandleDeleteServers()
        {
            if (this.Uri.Segments.Count() < 5)
            {
                throw new NotImplementedException();
            }

            var srvId = this.Uri.Segments[4].TrimEnd('/');
            var srv = this.Servers.FirstOrDefault(s => s.Id == srvId);
            if (srv == null)
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            switch (this.Uri.Segments.Count())
            {
                case 5:
                    this.Servers.Remove(srv);
                    return TestHelper.CreateResponse(HttpStatusCode.OK);
                case 7:
                    var key = this.Uri.Segments[6].TrimEnd('/');
                    if (!srv.Metadata.ContainsKey(key))
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }
                    srv.Metadata.Remove(key);
                    return TestHelper.CreateResponse(HttpStatusCode.OK);
                default:
                    throw new NotImplementedException();
            }
        }

        protected override IHttpResponseAbstraction HandleHead()
        {
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandleCopy()
        {
            throw new NotImplementedException();
        }

        private string GetPayload(Stream input)
        {
            using (var sr = new StreamReader(input))
            {
                return sr.ReadToEnd();
            }
        }

        private string GenerateItemsPayload(IEnumerable<ComputeItem> items, string collectionName )
        {
            var payload = new StringBuilder();
            payload.Append(string.Format("{{ \"{0}\": [",collectionName));
            var first = true;
            foreach (var item in items)
            {
                if (!first)
                {
                    payload.Append(",");
                }

                payload.Append("{");
                payload.Append(string.Format("\"id\": \"{0}\",", item.Id));
                payload.Append("\"links\": [");
                payload.Append("{");
                payload.Append(string.Format("\"href\": \"{0}\",", item.PublicUri.AbsoluteUri));
                payload.Append("\"rel\": \"self\"");
                payload.Append("},{");
                payload.Append(string.Format("\"href\": \"{0}\",", item.PermanentUri.AbsoluteUri));
                payload.Append("\"rel\": \"bookmark\"");
                payload.Append("}");
                payload.Append("],");
                payload.Append(string.Format("\"name\": \"{0}\"", item.Name));
                payload.Append("}");
                first = false;
            }
            payload.Append("] }");
            return payload.ToString();
        }

        private string GenerateKeyPairPayload(ComputeKeyPair keyPair)
        {
            var payloadFixture = @"{{
                ""keypair"": {{
                    ""public_key"": ""{1}"",
                    ""name"": ""{0}"",
                    ""fingerprint"": ""{2}""
                }}
            }}";
            return string.Format(payloadFixture, keyPair.Name, keyPair.PublicKey, keyPair.Fingerprint);
        }

        private string GenerateKeyPairsPayload(IEnumerable<ComputeKeyPair> keyPairs)
        {
            var payload = new StringBuilder();
            payload.Append("{ \"keypairs\": [");
            var first = true;
            foreach (var keyPair in keyPairs)
            {
                if (!first)
                {
                    payload.Append(",");
                }
                payload.Append(GenerateKeyPairPayload(keyPair));
                first = false;
            }

            payload.Append("] }");
            return payload.ToString();
        }

        private string GenerateFlavorPayload(ComputeFlavor flavor)
        {
            var payloadFixture = @"{{
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
            return string.Format(payloadFixture, flavor.Name, flavor.PublicUri.AbsoluteUri,
                flavor.PermanentUri.AbsoluteUri, flavor.Ram, flavor.Vcpus, flavor.Disk, flavor.Id);
        }

        private string GenerateImagePayload(ComputeImage image)
        {
            var payloadFixture = @"{{
                                    ""image"" : {{
                                        ""name"": ""{0}"",
                                        ""status"": ""{1}"",
                                        ""updated"": ""{2}"",
                                        ""created"": ""{3}"",
                                        ""minDisk"": {4},
                                        ""progress"": {5},
                                        ""minRam"": {6},
                                        ""links"": [
                                            {{
                                                ""href"": ""{7}"",
                                                ""rel"": ""self""
                                            }},
                                            {{
                                                ""href"": ""{8}"",
                                                ""rel"": ""bookmark""
                                            }}
                                        ],
                                        ""id"": ""{9}""
                                    }}
                                }}";

            return string.Format(payloadFixture, image.Name, image.Status, image.CreateDate, image.LastUpdated,
                image.MinimumDiskSize, image.UploadProgress, image.MinimumRamSize, image.PublicUri.AbsoluteUri,
                image.PermanentUri.AbsoluteUri, image.Id);
        }

        internal string GenerateCreateServeResponse(ComputeServer server)
        {
            var payloadFixture = @"{{
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
                            ""href"": ""{2}"",
                            ""rel"": ""self""
                        }},
                        {{
                            ""href"": ""{3}"",
                            ""rel"": ""bookmark""
                        }}
                    ],
                    ""adminPass"": ""{1}""
                }}
            }}";

            return string.Format(payloadFixture, server.Id, server.AdminPassword,
                server.PublicUri.AbsoluteUri, server.PermanentUri.AbsoluteUri);
        }

        private string GenerateServerPayload(ComputeServer server)
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
            return string.Format(payloadFixture, server.Id, server.Name, server.Status, server.Progress, server.PublicUri.AbsoluteUri, server.PermanentUri.AbsoluteUri);
        }

        private string GenerateMetadataPayload(IDictionary<string, string> metadata)
        {
            var payload = new StringBuilder();
            payload.Append("{ \"metadata\" : {");
            var isFirst = true;

            foreach (var item in metadata)
            {
                if (!isFirst)
                {
                    payload.Append(",");
                }

                payload.AppendFormat("\"{0}\":\"{1}\"", item.Key, item.Value);
                isFirst = false;
            }

            payload.Append("}}");
            return payload.ToString();
        }

        private void ParseAndStoreMetadata(MetadataComputeItem item, string payload)
        {
            var jObj = JObject.Parse(payload);
            var metaToken = jObj["metadata"];
            var metdata = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaToken.ToString());
            item.Metadata = metdata;
        }
    }

    public class ComputeRestSimulatorFactory : IHttpAbstractionClientFactory
    {
        internal ComputeRestSimulator Simulator = null;

        public ComputeRestSimulatorFactory(ComputeRestSimulator simulator)
        {
            this.Simulator = simulator;
        }

        public IHttpAbstractionClient Create()
        {
            throw new NotImplementedException();
        }

        public IHttpAbstractionClient Create(CancellationToken token)
        {
            if (this.Simulator != null)
            {
                this.Simulator.Headers.Clear();
            }
            return this.Simulator ?? new ComputeRestSimulator(token);
        }

        public IHttpAbstractionClient Create(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IHttpAbstractionClient Create(TimeSpan timeout, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
