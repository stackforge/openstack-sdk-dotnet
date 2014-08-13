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
using System.CodeDom.Compiler;
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
using OpenStack.Network;

namespace OpenStack.Test.Network
{
    public class NetworkRestSimulator : RestSimulator
    {
        internal ICollection<OpenStack.Network.Network> Networks { get; private set; }

        internal ICollection<FloatingIp> FloatingIps { get; private set; }

        public NetworkRestSimulator() : base()
        {
            this.Networks = new List<OpenStack.Network.Network>();
            this.FloatingIps = new List<OpenStack.Network.FloatingIp>();
        }

        public NetworkRestSimulator(CancellationToken token)
            : this()
        {
        }

        protected override IHttpResponseAbstraction HandleGet()
        {
            if (this.Uri.Segments.Count() >= 3)
            {
                switch (this.Uri.Segments[2].TrimEnd('/').ToLower())
                {
                    case "networks":
                        return HandleGetNetworks();
                    case "floatingips":
                        return HandleGetFloatingIps();
                }
            }
            throw new NotImplementedException();
        }

        internal IHttpResponseAbstraction HandleGetNetworks()
        {
            Stream networkContent;
            switch (this.Uri.Segments.Count())
            {
                case 3:
                    //no flavor id given, list all flavors
                    networkContent = TestHelper.CreateStream(GenerateCollectionPayload(this.Networks, this.GenerateNetworkPayload, "networks"));
                    break;
                default:
                    //Unknown Uri format
                    throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), networkContent);
        }

        internal IHttpResponseAbstraction HandleGetFloatingIps()
        {
            Stream floatingIpContent;
            switch (this.Uri.Segments.Count())
            {
                case 3:
                    floatingIpContent = TestHelper.CreateStream(GenerateCollectionPayload(this.FloatingIps, this.GenerateFloatingIpPayload, "floatingips"));
                    break;
                case 4:
                    var floatId = this.Uri.Segments[3].TrimEnd('/').ToLower();
                    return HandleGetFloatingIp(floatId);
                default:
                    //Unknown Uri format
                    throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), floatingIpContent);
        }

        internal IHttpResponseAbstraction HandleGetFloatingIp(string floatingIpId)
        {
            var payloadFixture = @"{{
                ""floatingip"": {0}
            }}";

            var floatIp = this.FloatingIps.FirstOrDefault(ip => ip.Id == floatingIpId);
            if (floatIp == null)
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            var floatIpContent = string.Format(payloadFixture, GenerateFloatingIpPayload(floatIp)).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), floatIpContent);
        }

        protected override IHttpResponseAbstraction HandlePost()
        {
            if (this.Uri.Segments.Count() >= 3)
            {
                switch (this.Uri.Segments[2].TrimEnd('/').ToLower())
                {
                    case "floatingips":
                        return HandleCreateFloatingIp();
                }
            }
            throw new NotImplementedException();
        }

        internal IHttpResponseAbstraction HandleCreateFloatingIp()
        {
            var payloadFixture = @"{{
                ""floatingip"": {0}
            }}";

            var ip = new FloatingIp(Guid.NewGuid().ToString(), "172.0.0." +(this.FloatingIps.Count +1), FloatingIpStatus.Active);
            this.FloatingIps.Add(ip);
            var floatIpContent = string.Format(payloadFixture, GenerateFloatingIpPayload(ip)).ConvertToStream();
            return TestHelper.CreateResponse(HttpStatusCode.Created, new Dictionary<string, string>(), floatIpContent);
        }

        protected override IHttpResponseAbstraction HandlePut()
        {
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandleDelete()
        {
            if (this.Uri.Segments.Count() >= 3)
            {
                switch (this.Uri.Segments[2].TrimEnd('/').ToLower())
                {
                    case "floatingips":
                        if (this.Uri.Segments.Count() == 4)
                        {
                            var floatId = this.Uri.Segments[3].TrimEnd('/').ToLower();
                            return HandleDeleteFloatingIps(floatId);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            throw new NotImplementedException();
        }

        internal IHttpResponseAbstraction HandleDeleteFloatingIps(string floatingIpId)
        {
            var floatIp = this.FloatingIps.FirstOrDefault(ip => ip.Id == floatingIpId);
            if (floatIp == null)
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            this.FloatingIps.Remove(floatIp);
            return TestHelper.CreateResponse(HttpStatusCode.NoContent, new Dictionary<string, string>());
        }


        protected override IHttpResponseAbstraction HandleHead()
        {
            throw new NotImplementedException();
        }

        protected override IHttpResponseAbstraction HandleCopy()
        {
            throw new NotImplementedException();
        }

        private string GenerateCollectionPayload<T>(IEnumerable<T> collection, Func<T, string> genFunc,
            string collectionName)
        {
            var payload = new StringBuilder();
            payload.Append(string.Format("{{ \"{0}\": [", collectionName));
            var first = true;
            foreach (var item in collection)
            {
                if (!first)
                {
                    payload.Append(",");
                }

                payload.Append(genFunc(item));
                first = false;
            }
            payload.Append("] }");
            return payload.ToString();
        }

        private string GenerateNetworkPayload(OpenStack.Network.Network network)
        {
            var payloadFixture = @"{{
                ""status"": ""{2}"",
                ""subnets"": [
                    ""d3839504-ec4c-47a4-b7c7-07af079a48bb""
                ],
                ""name"": ""{0}"",
                ""router:external"": false,
                ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                ""admin_state_up"": true,
                ""shared"": false,
                ""id"": ""{1}""
            }}";
            return string.Format(payloadFixture, network.Name, network.Id, network.Status.ToString().ToUpper());
        }

        private string GenerateFloatingIpPayload(FloatingIp floatingIp)
        {
            var payloadFixture = @"{{
                    ""router_id"": ""fafac59b-a94a-4525-8700-f4f448e0ac97"",
                    ""status"": ""{1}"",
                    ""tenant_id"": ""ffe683d1060449d09dac0bf9d7a371cd"",
                    ""floating_network_id"": ""3eaab3f7-d3f2-430f-aa73-d07f39aae8f4"",
                    ""fixed_ip_address"": ""10.0.0.2"",
                    ""floating_ip_address"": ""{2}"",
                    ""port_id"": ""9da94672-6e6b-446c-9579-3dd5484b31fd"",
                    ""id"": ""{0}""
                }}";
            return string.Format(payloadFixture, floatingIp.Id, floatingIp.Status.ToString().ToUpper(), floatingIp.FloatingIpAddress);
        }
    }

    public class NetworkRestSimulatorFactory : IHttpAbstractionClientFactory
    {
        internal NetworkRestSimulator Simulator = null;

        public NetworkRestSimulatorFactory(NetworkRestSimulator simulator)
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
            return this.Simulator ?? new NetworkRestSimulator(token);
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
