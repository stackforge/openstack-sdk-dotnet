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

using System.Collections.Generic;
using System.Threading;
using OpenStack;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace CustomServiceClientExample
{
    internal class EchoServiceClientDefinition : IOpenStackServiceClientDefinition
    {
        public string Name { get; private set; }

        public EchoServiceClientDefinition()
        {
            this.Name = typeof(EchoServiceClient).Name;
        }

        public IOpenStackServiceClient Create(ICredential credential, string serviceName, CancellationToken cancellationToken, IServiceLocator serviceLocator)
        {
            return new EchoServiceClient(credential, cancellationToken, serviceLocator);
        }

        public IEnumerable<string> ListSupportedVersions()
        {
            return new List<string>();
        }

        public bool IsSupported(ICredential credential, string serviceName)
        {
            return true;
        }
    }
}