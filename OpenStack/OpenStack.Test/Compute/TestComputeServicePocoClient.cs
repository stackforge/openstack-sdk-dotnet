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
using System.Threading.Tasks;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;

namespace OpenStack.Test.Compute
{
    public class TestComputeServicePocoClient : IComputeServicePocoClient
    {
        public Func<string, Task<ComputeFlavor>> GetFlavorDelegate { get; set; }

        public Func<Task<IEnumerable<ComputeFlavor>>> GetFlavorsDelegate { get; set; }

        public async Task<ComputeFlavor> GetFlavor(string flavorId)
        {
            return await this.GetFlavorDelegate(flavorId);
        }

        public async Task<IEnumerable<ComputeFlavor>> GetFlavors()
        {
            return await this.GetFlavorsDelegate();
        }
    }

    public class TestComputeServicePocoClientFactory : IComputeServicePocoClientFactory
    {
        internal IComputeServicePocoClient client;

        public TestComputeServicePocoClientFactory(IComputeServicePocoClient client)
        {
            this.client = client;
        }

        public IComputeServicePocoClient Create(ServiceClientContext context, IServiceLocator serviceLocator)
        {
            return client;
        }
    }
}
