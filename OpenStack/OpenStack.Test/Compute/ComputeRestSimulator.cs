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
using OpenStack.Common.Http;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    public class ComputeRestSimulator : RestSimulator
    {
        internal ICollection<ComputeFlavor> Flavors { get; private set; }
        internal ICollection<ComputeImage> Images { get; private set; }

        public ComputeRestSimulator() : base()
        {
            this.Flavors = new List<ComputeFlavor>();
            this.Images = new List<ComputeImage>();
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
                }
            }
            throw new NotImplementedException();
        }

        internal IHttpResponseAbstraction HandleGetFlavor()
        {
            Stream flavorContent;
            if (this.Uri.Segments.Count() == 5)
            {
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
            }
            else if (this.Uri.Segments.Count() == 4)
            {
                //no flavor id given, list all flavors
                flavorContent = TestHelper.CreateStream(GenerateItemsPayload(this.Flavors,"flavors"));
            }
            else
            {
                //Unknown Uri format
                throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), flavorContent);
        }

        internal IHttpResponseAbstraction HandleGetImage()
        {
            Stream imageContent;
            if (this.Uri.Segments.Count() == 5)
            {
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
            }
            else
            {
                //Unknown Uri format
                throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), imageContent);
        }

        protected override IHttpResponseAbstraction HandlePost()
        {
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandlePut()
        {
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandleDelete()
        {
            if (this.Uri.Segments.Count() >= 4)
            {
                if (this.Uri.Segments[3].TrimEnd('/').ToLower() == "images")
                {
                    if (this.Uri.Segments.Count() == 5)
                    {
                        var imageId = this.Uri.Segments[4].TrimEnd('/');
                        var image = this.Images.FirstOrDefault(i => i.Id == imageId);
                        if (image != null)
                        {
                            this.Images.Remove(image);
                            return TestHelper.CreateResponse(HttpStatusCode.OK);
                        }
                    }
                }
            }
            return TestHelper.CreateResponse(HttpStatusCode.NotFound);
        }

        protected override IHttpResponseAbstraction HandleHead()
        {
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandleCopy()
        {
            throw new NotImplementedException();
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
