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

using System.Collections.Generic;
using System.Threading;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack
{
    public interface IOpenStackServiceClientDefinition
    {
        /// <summary>
        /// The name of the service client.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates an instance of the service client being defined.
        /// </summary>
        /// <param name="credential">The credential that the client will use.</param>
        /// <param name="serviceName">The name of the service that the client will use.</param>
        /// <param name="cancellationToken">The cancellation token that the client will use.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        /// <returns></returns>
        IOpenStackServiceClient Create(ICredential credential, string serviceName, CancellationToken cancellationToken, IServiceLocator serviceLocator);

        /// <summary>
        /// Gets a list of supported versions.
        /// </summary>
        /// <returns>A list of versions.</returns>
        IEnumerable<string> ListSupportedVersions();

        /// <summary>
        /// Determines if this client is currently supported.
        /// </summary>
        /// <param name="credential">The credential for the service to use.</param>
        /// <param name="serviceName">The serviceName for the service.</param>
        /// <returns>A value indicating if the client is supported.</returns>
        bool IsSupported(ICredential credential, string serviceName);
    }
}
