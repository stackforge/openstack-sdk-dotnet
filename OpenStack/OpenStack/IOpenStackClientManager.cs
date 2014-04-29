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
using System.Threading;
using OpenStack.Identity;

namespace OpenStack
{
    /// <summary>
    /// Manages the creation and registration of OpenStack clients.
    /// </summary>
    public interface IOpenStackClientManager
    {
        /// <summary>
        /// Creates an instance of a client that can support the given credential.
        /// </summary>
        /// <param name="credential">The credential that must be supported.</param>
        /// <returns>An instance of an OpenStack client.</returns>
        IOpenStackClient CreateClient(ICredential credential);

        /// <summary>
        ///  Creates an instance of a client that can support the given credential and version.
        /// </summary>
        /// <param name="credential">The credential that must be supported.</param>
        /// <param name="version">The version that must be supported.</param>
        /// <returns>An instance of an OpenStack client.</returns>
        IOpenStackClient CreateClient(ICredential credential, string version);

        /// <summary>
        ///  Creates an instance of a client that can support the given credential and version.
        /// </summary>
        /// <param name="credential">The credential that must be supported.</param>
        /// <param name="token">A cancellation token that can be used to cancel operations.</param>
        /// <param name="version">The version that must be supported.</param>
        /// <returns>An instance of an OpenStack client.</returns>
        IOpenStackClient CreateClient(ICredential credential, CancellationToken token, string version);

        /// <summary>
        ///  Creates an instance of a client that can support the given credential and version.
        /// </summary>
        /// <param name="credential">The credential that must be supported.</param>
        /// <returns>An instance of an OpenStack client.</returns>
        IOpenStackClient CreateClient<T>(ICredential credential) where T : IOpenStackClient;

        /// <summary>
        ///  Creates an instance of a client that can support the given credential and version.
        /// </summary>
        /// <param name="credential">The credential that must be supported.</param>
        /// <param name="version">The version that must be supported.</param>
        /// <returns>An instance of an OpenStack client.</returns>
        IOpenStackClient CreateClient<T>(ICredential credential, string version) where T : IOpenStackClient;

        /// <summary>
        ///  Creates an instance of a client that can support the given credential and version.
        /// </summary>
        /// <param name="credential">The credential that must be supported.</param>
        /// <param name="token">A cancellation token that can be used to cancel operations.</param>
        /// <param name="version">The version that must be supported.</param>
        /// <returns>An instance of an OpenStack client.</returns>
        IOpenStackClient CreateClient<T>(ICredential credential, CancellationToken token, string version) where T : IOpenStackClient;

        /// <summary>
        /// Registers a client for use.
        /// </summary>
        /// <typeparam name="T">The type of the client to be registered.</typeparam>
        void RegisterClient<T>() where T : IOpenStackClient;

        /// <summary>
        /// Gets a list of available clients that are being managed by this object.
        /// </summary>
        /// <returns>A list of OpenStack clients.</returns>
        IEnumerable<Type> ListAvailableClients();
    }
}