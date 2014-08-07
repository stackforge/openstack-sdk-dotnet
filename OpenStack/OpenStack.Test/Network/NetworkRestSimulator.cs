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
using OpenStack.Network;

namespace OpenStack.Test.Network
{
    public class NetworkRestSimulator : RestSimulator
    {
        internal ICollection<OpenStack.Network.Network> Networks { get; private set; }

        public NetworkRestSimulator() : base()
        {
            this.Networks = new List<OpenStack.Network.Network>();
        }

        public NetworkRestSimulator(CancellationToken token)
            : this()
        {
        }

        protected override IHttpResponseAbstraction HandleGet()
        {
            if (this.Uri.Segments.Count() >= 4)
            {
                switch (this.Uri.Segments[3].TrimEnd('/').ToLower())
                {
                    case "networks":
                        return HandleGetNetworks();
                }
            }
            throw new NotImplementedException();
        }

        internal IHttpResponseAbstraction HandleGetNetworks()
        {
            Stream networkContent;
            switch (this.Uri.Segments.Count())
            {
                case 4:
                    //no flavor id given, list all flavors
                    networkContent = TestHelper.CreateStream(GenerateNetworksPayload(this.Networks));
                    break;
                default:
                    //Unknown Uri format
                    throw new NotImplementedException();
            }

            return TestHelper.CreateResponse(HttpStatusCode.OK, new Dictionary<string, string>(), networkContent);
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

        private string GenerateNetworksPayload(IEnumerable<OpenStack.Network.Network> networks)
        {
            var payload = new StringBuilder();
            payload.Append("{{ \"networks\": [");
            var first = true;
            foreach (var item in networks)
            {
                if (!first)
                {
                    payload.Append(",");
                }

                payload.Append(GenerateNetworkPayload(item));
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
