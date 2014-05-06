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
using System.Linq;
using System.Reflection;
using System.Threading;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack
{
    /// <inheritdoc/>
    internal class OpenStackClientManager : IOpenStackClientManager
    {
        internal IServiceLocator ServiceLocator;
        internal ICollection<Type> clients;

        /// <summary>
        /// Creates a new instance of the OpenStackClientManager class.
        /// </summary>
        internal OpenStackClientManager(IServiceLocator serviceLocator)
        {
            this.ServiceLocator = serviceLocator;
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
            return this.CreateClient(credential, CancellationToken.None, version);
        }

        /// <inheritdoc/>
        public IOpenStackClient CreateClient(ICredential credential, CancellationToken token, string version)
        {
            credential.AssertIsNotNull("credential", "Cannot create an OpenStack client with a null credential.");
            version.AssertIsNotNull("version", "Cannot create an OpenStack client with a null version.");

            //Ensure that the assembly that contains the credential has a chance to register itself.
            this.ServiceLocator.EnsureAssemblyRegistration(credential.GetType().GetAssembly());

            return this.GetSupportedClient(this.clients, credential, token, version);
        }

        /// <inheritdoc/>
        public IOpenStackClient CreateClient<T>(ICredential credential) where T : IOpenStackClient
        {
            return this.CreateClient<T>(credential, CancellationToken.None, string.Empty);
        }

        /// <inheritdoc/>
        public IOpenStackClient CreateClient<T>(ICredential credential, string version) where T : IOpenStackClient
        {
            return this.CreateClient<T>(credential, CancellationToken.None, version);
        }

        /// <inheritdoc/>
        public IOpenStackClient CreateClient<T>(ICredential credential, CancellationToken token, string version) where T: IOpenStackClient
        {
            credential.AssertIsNotNull("credential", "Cannot create an OpenStack client with a null credential.");
            version.AssertIsNotNull("version", "Cannot create an OpenStack client with a null version.");

            //Ensure that the assemblies that contain the credential and client type has had a chance to register itself.
            this.ServiceLocator.EnsureAssemblyRegistration(credential.GetType().GetAssembly());
            this.ServiceLocator.EnsureAssemblyRegistration(typeof(T).GetAssembly());

            return this.GetSupportedClient(this.clients.Where(c => c == typeof(T)), credential, token, version);
        }

        /// <summary>
        /// Gets a client for the given collection that supports the credential and version.
        /// </summary>
        /// <param name="clientTypes">A list client types.</param>
        /// <param name="credential">A credential that needs to be supported.</param>
        /// <param name="version">A version that needs to be supported.</param>
        /// <param name="token">A cancellation token that can be used to cancel operations.</param>
        /// <returns>A client that supports the given credential and version.</returns>
        internal IOpenStackClient GetSupportedClient(IEnumerable<Type> clientTypes, ICredential credential, CancellationToken token, string version)
        {
            foreach (var clientType in clientTypes)
            {
                var client = this.CreateClientInstance(clientType, credential, token);
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
        /// <param name="credential">A credential that needs to be supported.</param>
        /// <param name="token">A cancellation token that can be used to cancel operations.</param>
        /// <returns>An instance of the requested client.</returns>
        internal IOpenStackClient CreateClientInstance(Type clientType, ICredential credential, CancellationToken token)
        {
            clientType.AssertIsNotNull("clientType", "Cannot create an OpenStack client with a null type.");
            credential.AssertIsNotNull("credential", "Cannot create an OpenStack client with a null credential.");
            token.AssertIsNotNull("credential", "Cannot create an OpenStack client with a null cancellation token. Use CancellationToken.None.");

            IOpenStackClient instance;
            try
            {
                instance = Activator.CreateInstance(clientType, credential, token, this.ServiceLocator) as IOpenStackClient;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Could not create a client of type '{0}'. See inner exception for details.", clientType.Name), ex);
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
