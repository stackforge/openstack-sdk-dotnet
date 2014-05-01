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

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenStack.Common.ServiceLocation;

namespace CustomServiceClientExample
{
    internal class EchoPocoClient : IEchoPocoClient
    {
        internal IServiceLocator ServiceLocator;

        public EchoPocoClient(IServiceLocator serviceLocator)
        {
            this.ServiceLocator = serviceLocator;
        }

        public async Task<EchoResponse> Echo(string message)
        {
            var restClient = this.ServiceLocator.Locate<IEchoRestClientFactory>().Create(this.ServiceLocator);
            var resp = await restClient.Echo(message);
            var payload = await resp.ReadContentAsStringAsync();
            var obj = JObject.Parse(payload);
            return new EchoResponse((string)obj["args"]["m"], (string)obj["url"]);
        }
    }
}