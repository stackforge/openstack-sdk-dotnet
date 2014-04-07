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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Common;
using OpenStack.Common.Http;

namespace OpenStack.Test.Identity
{
    public class IdentityRestServiceSimulator : DisposableClass, IHttpAbstractionClient
    {
        #region Authentication Json Response Fixture

        internal string AuthJsonResponseFixture = @"{
                                                ""access"": {
                                                    ""token"": {
                                                        ""expires"": ""2014-03-18T10:59:46.355Z"",
                                                        ""id"": ""HPAuth10_af3d1bfe456d18e8d4793e54922f839fa051d9f60f115aca52c9a44f9e3d96fb"",
                                                        ""tenant"": {
                                                            ""id"": ""10244656540440"",
                                                            ""name"": ""10255892528404-Project""
                                                        }
                                                    },
                                                    ""user"": {
                                                        ""id"": ""10391119133001"",
                                                        ""name"": ""wayne.foley"",
                                                        ""otherAttributes"": {
                                                            ""domainStatus"": ""enabled"",
                                                            ""domainStatusCode"": ""00""
                                                        },
                                                        ""roles"": [ ]
                                                    },
                                                    ""serviceCatalog"": [
                                                        {
                                                            ""name"": ""Networking"",
                                                            ""type"": ""network"",
                                                            ""endpoints"": [
                                                                {
                                                                    ""tenantId"": ""10244656540440"",
                                                                    ""publicURL"": ""https://region-a.geo-1.network.hpcloudsvc.com"",
                                                                    ""publicURL2"": """",
                                                                    ""region"": ""region-a.geo-1"",
                                                                    ""versionId"": ""2.0"",
                                                                    ""versionInfo"": ""https://region-a.geo-1.network.hpcloudsvc.com"",
                                                                    ""versionList"": ""https://region-a.geo-1.network.hpcloudsvc.com""
                                                                }
                                                            ]
                                                        },
                                                        {
                                                            ""name"": ""Object Storage"",
                                                            ""type"": ""object-store"",
                                                            ""endpoints"": [
                                                                {
                                                                    ""tenantId"": ""10244656540440"",
                                                                    ""publicURL"": ""https://region-a.geo-1.objects.hpcloudsvc.com/v1/10244656540440"",
                                                                    ""region"": ""region-a.geo-1"",
                                                                    ""versionId"": ""1.0"",
                                                                    ""versionInfo"": ""https://region-a.geo-1.objects.hpcloudsvc.com/v1.0/"",
                                                                    ""versionList"": ""https://region-a.geo-1.objects.hpcloudsvc.com""
                                                                },
                                                                {
                                                                    ""tenantId"": ""10244656540440"",
                                                                    ""publicURL"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443/v1/10244656540440"",
                                                                    ""region"": ""region-b.geo-1"",
                                                                    ""versionId"": ""1"",
                                                                    ""versionInfo"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443/v1/"",
                                                                    ""versionList"": ""https://region-b.geo-1.objects.hpcloudsvc.com:443""
                                                                }
                                                            ]
                                                        },
                                                        {
                                                            ""name"": ""Identity"",
                                                            ""type"": ""identity"",
                                                            ""endpoints"": [
                                                                {
                                                                    ""publicURL"": ""https://region-a.geo-1.identity.hpcloudsvc.com:35357/v2.0/"",
                                                                    ""region"": ""region-a.geo-1"",
                                                                    ""versionId"": ""2.0"",
                                                                    ""versionInfo"": ""https://region-a.geo-1.identity.hpcloudsvc.com:35357/v2.0/"",
                                                                    ""versionList"": ""https://region-a.geo-1.identity.hpcloudsvc.com:35357""
                                                                },
                                                                {
                                                                    ""publicURL"": ""https://region-a.geo-1.identity.hpcloudsvc.com:35357/v3/"",
                                                                    ""region"": ""region-a.geo-1"",
                                                                    ""versionId"": ""3.0"",
                                                                    ""versionInfo"": ""https://region-a.geo-1.identity.hpcloudsvc.com:35357/v3/"",
                                                                    ""versionList"": ""https://region-a.geo-1.identity.hpcloudsvc.com:35357""
                                                                },
                                                                {
                                                                    ""publicURL"": ""https://region-b.geo-1.identity.hpcloudsvc.com:35357/v2.0/"",
                                                                    ""region"": ""region-b.geo-1"",
                                                                    ""versionId"": ""2.0"",
                                                                    ""versionInfo"": ""https://region-b.geo-1.identity.hpcloudsvc.com:35357/v2.0/"",
                                                                    ""versionList"": ""https://region-b.geo-1.identity.hpcloudsvc.com:35357""
                                                                },
                                                                {
                                                                    ""publicURL"": ""https://region-b.geo-1.identity.hpcloudsvc.com:35357/v3/"",
                                                                    ""region"": ""region-b.geo-1"",
                                                                    ""versionId"": ""3.0"",
                                                                    ""versionInfo"": ""https://region-b.geo-1.identity.hpcloudsvc.com:35357/v3/"",
                                                                    ""versionList"": ""https://region-b.geo-1.identity.hpcloudsvc.com:35357""
                                                                }
                                                            ]
                                                        },
                                                        {
                                                            ""name"": ""Compute"",
                                                            ""type"": ""compute"",
                                                            ""endpoints"": [
                                                                {
                                                                    ""tenantId"": ""10244656540440"",
                                                                    ""publicURL"": ""https://region-a.geo-1.compute.hpcloudsvc.com/v2/10244656540440"",
                                                                    ""region"": ""region-a.geo-1"",
                                                                    ""versionId"": ""2"",
                                                                    ""versionInfo"": ""https://region-a.geo-1.compute.hpcloudsvc.com/v2/"",
                                                                    ""versionList"": ""https://region-a.geo-1.compute.hpcloudsvc.com""
                                                                }
                                                            ]
                                                        }
                                                    ]
                                                }
                                            }";


        #endregion

        public HttpMethod Method { get; set; }
        public Uri Uri { get; set; }
        public Stream Content { get; set; }
        public IDictionary<string, string> Headers { get; private set; }
        public string ContentType { get; set; }
        public TimeSpan Timeout { get; set; }
        //public event EventHandler<HttpProgressEventArgs> HttpReceiveProgress;
        //public event EventHandler<HttpProgressEventArgs> HttpSendProgress;

        public IdentityRestServiceSimulator()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public Task<IHttpResponseAbstraction> SendAsync()
        {
            var resp = TestHelper.CreateResponse(HttpStatusCode.OK, new List<KeyValuePair<string, string>>(),
                this.AuthJsonResponseFixture.ConvertToStream());
            return Task.Factory.StartNew(() => resp);
        }
    }

    public class IdentityRestServiceSimulatorFactory : IHttpAbstractionClientFactory
    {
        internal IdentityRestServiceSimulator Simulator;

        public IdentityRestServiceSimulatorFactory(IdentityRestServiceSimulator simulator)
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
            return this.Simulator ?? new IdentityRestServiceSimulator();
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
