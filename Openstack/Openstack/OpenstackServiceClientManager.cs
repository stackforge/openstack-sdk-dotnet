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
    using System.Linq;
    using System.Threading;
    using Openstack.Common;
    using Openstack.Identity;

    /// <inheritdoc/>
    internal class OpenstackServiceClientManager : IOpenstackServiceClientManager
    {
        internal IDictionary<Type, IOpenstackServiceClientDefinition> serviceClientDefinitions;

        /// <summary>
        /// Creates a new instance of the OpenstackServiceClientManager class.
        /// </summary>
        internal OpenstackServiceClientManager()
        {
            this.serviceClientDefinitions = new Dictionary<Type, IOpenstackServiceClientDefinition>();
        }

        /// <inheritdoc/>
        public T CreateServiceClient<T>(ICredential credential, CancellationToken cancellationToken) where T : IOpenstackServiceClient
        {
            credential.AssertIsNotNull("credential", "Cannot create an Openstack service with a null credential.");
            cancellationToken.AssertIsNotNull("cancellationToken", "Cannot create an Openstack service with a null cancellationToken.");
            credential.ServiceCatalog.AssertIsNotNull("credential.ServiceCatalog", "Cannot create an Openstack service with a null service catalog.");

            foreach (var serviceClientDef in this.serviceClientDefinitions.Where(s =>typeof(T).IsAssignableFrom(s.Key)))
            {
                if (serviceClientDef.Value != null && serviceClientDef.Value.IsSupported(credential))
                {
                    var client = this.CreateServiceClientInstance(serviceClientDef.Value, credential, cancellationToken);
                    return (T) client;
                }
            }

            throw new InvalidOperationException("A client that supports the requested service for the given instance of Openstack could not be found.");
        }

        /// <summary>
        /// Creates an instance of the given client type using the given factory function, credential, and cancellation token.
        /// </summary>
        /// <param name="clientDefinition">A object that can be used to validate and create the give client type.</param>
        /// <param name="credential">The credential to be used by the created client.</param>
        /// <param name="cancellationToken">The cancellation token to be used by the created client.</param>
        /// <returns>An instance of the requested client.</returns>
        internal IOpenstackServiceClient CreateServiceClientInstance(IOpenstackServiceClientDefinition clientDefinition, ICredential credential, CancellationToken cancellationToken)
        {
            clientDefinition.AssertIsNotNull("clientDefinition", "Cannot create an Openstack service with a null client definition.");

            IOpenstackServiceClient instance;
            try
            {
                instance = clientDefinition.Create(credential, cancellationToken) as IOpenstackServiceClient;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Could not create a service of type '{0}'. See inner exception for details.", clientDefinition.Name), ex);
            }

            if (instance != null)
            {
                return instance;
            }
            throw new InvalidOperationException(string.Format("Could not create a service of type '{0}'. The type does not derive from or cast to IOpenstackClient. ", clientDefinition.Name));
        }

        /// <inheritdoc/>
        public IEnumerable<Type> ListAvailableServiceClients()
        {
            return this.serviceClientDefinitions.Keys;
        }

        /// <inheritdoc/>
        public void RegisterServiceClient<T>(IOpenstackServiceClientDefinition clientDefinition) where T : IOpenstackServiceClient
        {
            var servicetType = typeof (T);

            if (this.serviceClientDefinitions.ContainsKey(servicetType))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "A service of type '{0}' has already been registered, and cannot be registered again.",
                        servicetType.Name));
            }

            this.serviceClientDefinitions.Add(servicetType, clientDefinition);
        }
    }
}
