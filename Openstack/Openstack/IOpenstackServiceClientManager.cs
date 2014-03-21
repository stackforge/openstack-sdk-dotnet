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

namespace Openstack
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Openstack.Identity;

    /// <summary>
    /// Creates and registers clients that can be used to interact with Openstack services.
    /// </summary>
    public interface IOpenstackServiceClientManager
    {
        /// <summary>
        /// Create a client that can interact with the requested Openstack service.
        /// </summary>
        /// <typeparam name="T">The type of client to be created.</typeparam>
        /// <param name="credential">The credential to be used by the client.</param>
        /// <param name="cancellationToken">The cancellation token to be used by the client.</param>
        /// <returns>An instance of the requested client.</returns>
        T CreateServiceClient<T>(ICredential credential, CancellationToken cancellationToken) where T : IOpenstackServiceClient;

        /// <summary>
        /// Gets a list of all available clients that can be used to interact with Openstack services.
        /// </summary>
        /// <returns>A list of types of clients.</returns>
        IEnumerable<Type> ListAvailableServiceClients();

        /// <summary>
        /// Registers a client for use.
        /// </summary>
        /// <typeparam name="T">The type of the client to register.</typeparam>
        /// <param name="serviceClientDefinition">An object that can be used to validate support and construct the given client.</param>
        void RegisterServiceClient<T>(IOpenstackServiceClientDefinition serviceClientDefinition) where T : IOpenstackServiceClient;
    }
}
