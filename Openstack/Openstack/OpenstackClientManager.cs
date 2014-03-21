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
    using System.Collections.ObjectModel;
    using Openstack.Common;
    using Openstack.Identity;

    /// <inheritdoc/>
    internal class OpenstackClientManager : IOpenstackClientManager
    {
        internal ICollection<Type> clients;

        /// <summary>
        /// Creates a new instance of the OpenstackClientManager class.
        /// </summary>
        internal OpenstackClientManager()
        {
            this.clients = new Collection<Type>();
        }

        /// <inheritdoc/>
        public IOpenstackClient CreateClient(ICredential credential)
        {
            return this.CreateClient(credential, string.Empty);
        }

        /// <inheritdoc/>
        public IOpenstackClient CreateClient(ICredential credential, string version)
        {
            credential.AssertIsNotNull("credential","Cannot create an Openstack client with a null credential.");
            version.AssertIsNotNull("version", "Cannot create an Openstack client with a null version.");

            foreach (var clientType in this.clients)
            {
                var client = this.CreateClient(clientType);
                if (client.IsSupported(credential, version))
                {
                    return client;
                }
            }

            throw new InvalidOperationException("An Openstack client that supports the given credentials and version could not be found.");
        }

        /// <summary>
        /// Creates a new instance of the requested client type
        /// </summary>
        /// <param name="clientType">The type of the client to create.</param>
        /// <returns>An instance of the requested client.</returns>
        internal IOpenstackClient CreateClient(Type clientType)
        {
            clientType.AssertIsNotNull("clientType", "Cannot create an Openstack client with a null type.");

            IOpenstackClient instance; 
            try
            {
                instance = Activator.CreateInstance(clientType) as IOpenstackClient;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Could not create a client of type '{0}'. See inner exception for details.", clientType.Name),ex);
            }

            if (instance != null)
            {
                return instance;
            }
            throw new InvalidOperationException(string.Format("Could not create a client of type '{0}'. The type does not derive from or cast to IOpenstackClient. ", clientType.Name));
        }

        /// <inheritdoc/>
        public void RegisterClient<T>() where T: IOpenstackClient
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
