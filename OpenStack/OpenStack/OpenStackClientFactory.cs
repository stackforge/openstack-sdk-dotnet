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

using System.Threading;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack
{
    /// <summary>
    /// Factory class for creating OpenStack clients.
    /// </summary>
    public static class OpenStackClientFactory
    {
        /// <summary>
        /// Creates an OpenStack client that supports the given credential.
        /// </summary>
        /// <param name="credential">A credential to be used by the client.</param>
        /// <returns>A OpenStack client.</returns>
        public static IOpenStackClient CreateClient(IOpenStackCredential credential)
        {
            return CreateClient(credential, CancellationToken.None, string.Empty);
        }

        /// <summary>
        /// Creates an OpenStack client that supports the given credential.
        /// </summary>
        /// <param name="credential">A credential to be used by the client.</param>
        /// <param name="version">A version that the client must support.</param>
        /// <returns>A OpenStack client.</returns>
        public static IOpenStackClient CreateClient(IOpenStackCredential credential, string version)
        {
            return CreateClient(credential, CancellationToken.None, version);
        }

        /// <summary>
        /// Creates an OpenStack client that supports the given credential and version.
        /// </summary>
        /// <param name="credential">A credential to be used by the client.</param>
        /// <param name="version">A version that the client must support.</param>
        /// <param name="token">A cancellation token to be used to cancel operations.</param>
        /// <returns>An OpenStack client.</returns>
        public static IOpenStackClient CreateClient(IOpenStackCredential credential, CancellationToken token, string version)
        {
            credential.AssertIsNotNull("credential", "Cannot create a client with a null credential.");

            var locator = new ServiceLocator();
            var clientManager = locator.Locate<IOpenStackClientManager>();
            return clientManager.CreateClient(credential, token, version);
        }

        /// <summary>
        /// Creates a client of the requested type that supports the given credential.
        /// </summary>
        /// <typeparam name="T">The type of client to create.</typeparam>
        /// <param name="credential">A credential to be used by the client.</param>
        /// <returns>An OpenStack client.</returns>
        public static IOpenStackClient CreateClient<T>(IOpenStackCredential credential) where T : IOpenStackClient
        {
            return CreateClient<T>(credential, CancellationToken.None, string.Empty);
        }

        /// <summary>
        /// Creates a client of the requested type that supports the given credential.
        /// </summary>
        /// <typeparam name="T">The type of client to create.</typeparam>
        /// <param name="credential">A credential to be used by the client.</param>
        /// <param name="version">A version that the client must support.</param>
        /// <returns>An OpenStack client.</returns>
        public static IOpenStackClient CreateClient<T>(IOpenStackCredential credential, string version) where T : IOpenStackClient
        {
            return CreateClient<T>(credential, CancellationToken.None, version);
        }

        /// <summary>
        /// Creates a client of the requested type that supports the given credential and version.
        /// </summary>
        /// <typeparam name="T">The type of client to create.</typeparam>
        /// <param name="credential">A credential to be used by the client.</param>
        /// <param name="version">A version that the client must support.</param>
        /// <param name="token">A cancellation token to be used to cancel operations.</param>
        /// <returns>An OpenStack client.</returns>
        public static IOpenStackClient CreateClient<T>(IOpenStackCredential credential, CancellationToken token, string version) where T : IOpenStackClient
        {
            credential.AssertIsNotNull("credential", "Cannot create a client with a null credential.");

            var locator = new ServiceLocator();
            var clientManager = locator.Locate<IOpenStackClientManager>();
            return clientManager.CreateClient<T>(credential, token, version);
        }
    }
}
