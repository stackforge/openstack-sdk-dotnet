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

        public ComputeRestSimulator() : base()
        {
            this.Flavors = new List<ComputeFlavor>();
            this.Images = new List<ComputeImage>();
            this.Servers = new List<ComputeServer>();
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
                    serverContent = TestHelper.CreateStream(GenerateItemsPayload(this.Flavors, "flavors"));
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
                    if (this.Uri.Segments[5].TrimEnd('/').ToLower() != "metadata")
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }
                    var srvId = this.Uri.Segments[4].TrimEnd('/');
                    var srv =
                        this.Servers.FirstOrDefault(
                            s => string.Equals(s.Id, srvId, StringComparison.OrdinalIgnoreCase));
                    if (srv == null)
                    {
                        return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                    }

                    ParseAndStoreMetadata(srv, GetPayload(this.Content));
                    break;
                default:
                    throw new NotImplementedException();
            }

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
            throw new NotImplementedException();
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
