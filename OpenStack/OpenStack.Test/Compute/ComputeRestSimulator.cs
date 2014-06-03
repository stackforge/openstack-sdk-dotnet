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
using System.Text;
using System.Threading;
using OpenStack.Common.Http;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    public class ComputeRestSimulator : RestSimulator
    {
        internal ICollection<ComputeFlavor> Flavors { get; private set; }

        public ComputeRestSimulator() : base()
        {
            this.Flavors = new List<ComputeFlavor>();
        }

        public ComputeRestSimulator(CancellationToken token) : this()
        {
        }

        protected override IHttpResponseAbstraction HandleGet()
        {
            if (this.Uri.Segments.Count() >= 4)
            {
                if (this.Uri.Segments[3].TrimEnd('/').ToLower() == "flavors")
                {
                    Stream flavorContent;
                    if (this.Uri.Segments.Count() == 5)
                    {
                        //flavor id given, list single flavor
                        var flavorId = this.Uri.Segments[4].TrimEnd('/');
                        var flavor =
                            this.Flavors.FirstOrDefault(
                                f => string.Equals(f.Id, flavorId, StringComparison.OrdinalIgnoreCase));
                        if(flavor == null)
                        {
                            return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                        }

                        flavorContent = TestHelper.CreateStream(GenerateFlavorPayload(flavor));
                    }
                    else if (this.Uri.Segments.Count() == 4)
                    {
                        //no flavor id given, list all flavors
                        flavorContent = TestHelper.CreateStream(GenerateFlavorsPayload());
                    }
                    else
                    {
                        //Unknown Uri format
                        throw new NotImplementedException();
                    }

                    return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string,string>(), flavorContent);
                }
            }
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandleHead()
        {
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandleCopy()
        {
            throw new NotImplementedException();
        }

        private string GenerateFlavorsPayload()
        {
            var payload = new StringBuilder();
            payload.Append("{ \"flavors\": [");
            var first = true;
            foreach (var flavor in this.Flavors)
            {
                if (!first)
                {
                    payload.Append(",");
                }

                payload.Append("{");
                payload.Append(string.Format("\"id\": \"{0}\",", flavor.Id));
                payload.Append("\"links\": [");
                payload.Append("{");
                payload.Append(string.Format("\"href\": \"{0}\",", flavor.PublicUri.AbsoluteUri));
                payload.Append("\"rel\": \"self\"");
                payload.Append("},{");
                payload.Append(string.Format("\"href\": \"{0}\",", flavor.PermanentUri.AbsoluteUri));
                payload.Append("\"rel\": \"bookmark\"");
                payload.Append("}");
                payload.Append("],");
                payload.Append(string.Format("\"name\": \"{0}\"", flavor.Name));
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
