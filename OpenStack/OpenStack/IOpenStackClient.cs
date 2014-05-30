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
using System.Threading.Tasks;
using OpenStack.Identity;

namespace OpenStack
{
    /// <summary>
    /// Top level OpenStack client used to connect and interact with an instance of OpenStack.
    /// </summary>
    public interface IOpenStackClient
    {
        /// <summary>
        /// Gets a reference to the credential currently being used.
        /// </summary>
        IOpenStackCredential Credential { get; }

        /// <summary>
        /// Connects the client to the remote instance of OpenStack.
        /// </summary>
        /// <returns></returns>
        Task Connect();

        /// <summary>
        /// Changes the default region for the client.
        /// </summary>
        /// <param name="region">The region to be set.</param>
        /// <returns></returns>
        void SetRegion(string region);

        /// <summary>
        /// Creates a client for a given OpenStack service.
        /// </summary>
        /// <typeparam name="T">The type of client to create.</typeparam>
        /// <returns>An implementation of the requested client.</returns>
        T CreateServiceClient<T>() where T : IOpenStackServiceClient;

        /// <summary>
        /// Creates a client for a given OpenStack service that supports the given version.
        /// </summary>
        /// <typeparam name="T">The type of client to create.</typeparam>
        /// <param name="version">The version that must be supported.</param>
        /// <returns>An implementation of the requested client.</returns>
        T CreateServiceClient<T>(string version) where T : IOpenStackServiceClient;

        /// <summary>
        /// Creates a client for a given OpenStack service.
        /// </summary>
        /// <typeparam name="T">The type of client to create.</typeparam>
        /// <param name="serviceName">The name of the service that must be supported.</param>
        /// <returns>An implementation of the requested client.</returns>
        T CreateServiceClientByName<T>(string serviceName) where T : IOpenStackServiceClient;

        /// <summary>
        /// Creates a client for a given OpenStack service that supports the given version.
        /// </summary>
        /// <typeparam name="T">The type of client to create.</typeparam>
        /// <param name="serviceName">The name of the service that must be supported.</param>
        /// <param name="version">The version that must be supported.</param>
        /// <returns>An implementation of the requested client.</returns>
        T CreateServiceClientByName<T>(string serviceName, string version) where T : IOpenStackServiceClient;

        /// <summary>
        /// Gets a list of supported OpenStack versions for this client.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSupportedVersions();

        /// <summary>
        /// Determines if the client can support the given credential and version.
        /// </summary>
        /// <param name="credential">The credential that must be supported.</param>
        /// <param name="version">The version that must be supported.</param>
        /// <returns>A value indicating if the given credential and version are supported.</returns>
        bool IsSupported(ICredential credential, string version);
    }
}
