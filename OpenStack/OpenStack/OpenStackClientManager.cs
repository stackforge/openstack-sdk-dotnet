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
using System.Collections.ObjectModel;
using OpenStack.Common;
using OpenStack.Identity;

namespace OpenStack
{
    /// <inheritdoc/>
    internal class OpenStackClientManager : IOpenStackClientManager
    {
        internal ICollection<Type> clients;

        /// <summary>
        /// Creates a new instance of the OpenStackClientManager class.
        /// </summary>
        internal OpenStackClientManager()
        {
            this.clients = new Collection<Type>();
        }

        /// <inheritdoc/>
        public IOpenStackClient CreateClient(ICredential credential)
        {
            return this.CreateClient(credential, string.Empty);
        }

        /// <inheritdoc/>
        public IOpenStackClient CreateClient(ICredential credential, string version)
        {
            credential.AssertIsNotNull("credential","Cannot create an OpenStack client with a null credential.");
            version.AssertIsNotNull("version", "Cannot create an OpenStack client with a null version.");

            foreach (var clientType in this.clients)
            {
                var client = this.CreateClient(clientType);
                if (client.IsSupported(credential, version))
                {
                    return client;
                }
            }

            throw new InvalidOperationException("An OpenStack client that supports the given credentials and version could not be found.");
        }

        /// <summary>
        /// Creates a new instance of the requested client type
        /// </summary>
        /// <param name="clientType">The type of the client to create.</param>
        /// <returns>An instance of the requested client.</returns>
        internal IOpenStackClient CreateClient(Type clientType)
        {
            clientType.AssertIsNotNull("clientType", "Cannot create an OpenStack client with a null type.");

            IOpenStackClient instance; 
            try
            {
                instance = Activator.CreateInstance(clientType) as IOpenStackClient;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Could not create a client of type '{0}'. See inner exception for details.", clientType.Name),ex);
            }

            if (instance != null)
            {
                return instance;
            }
            throw new InvalidOperationException(string.Format("Could not create a client of type '{0}'. The type does not derive from or cast to IOpenStackClient. ", clientType.Name));
        }

        /// <inheritdoc/>
        public void RegisterClient<T>() where T: IOpenStackClient
        {
            var clientType = typeof (T);

            if (this.clients.Contains(clientType))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "A client of type '{0}' has already been registered, and cannot be registered again.",
                        clientType.Name));
            }

            this.clients.Add(clientType);
        }

        /// <inheritdoc/>
        public IEnumerable<Type> ListAvailableClients()
        {
            return this.clients;
        }
    }
}
