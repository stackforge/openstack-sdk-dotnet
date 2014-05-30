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
using System.Threading.Tasks;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;
using OpenStack.Identity;

namespace OpenStack
{
    /// <inheritdoc/>
    public class OpenStackClient : IOpenStackClient
    {
        internal CancellationToken CancellationToken;
        internal IServiceLocator ServiceLocator;

        /// <inheritdoc/>
        public IOpenStackCredential Credential { get; private set; }

        internal OpenStackClient()
        {
            
        }

        /// <summary>
        ///  Creates a new instance of the OpenStackClient class.
        /// </summary>
        /// <param name="credential">The credential to be used by this client.</param>
        /// <param name="cancellationToken">The cancellation token to be used by this client.</param>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        public OpenStackClient(IOpenStackCredential credential, CancellationToken cancellationToken, IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create an OpenStack client with a null service locator.");

            this.Credential = credential;
            this.CancellationToken = cancellationToken;
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public async Task Connect()
        {
            var identityClient = this.CreateServiceClient<IIdentityServiceClient>();
            this.Credential = await identityClient.Authenticate();
        }

        /// <inheritdoc/>
        public void SetRegion(string region)
        {
            region.AssertIsNotNullOrEmpty("region", "Cannot set the region on the client. Region must not be null or empty");
            this.Credential.SetRegion(region);
        }

        /// <inheritdoc/>
        public T CreateServiceClient<T>() where T : IOpenStackServiceClient
        {
            return this.CreateServiceClient<T>(string.Empty);
        }

        /// <inheritdoc/>
        public T CreateServiceClient<T>(string version) where T : IOpenStackServiceClient
        {
            var manager = this.ServiceLocator.Locate<IOpenStackServiceClientManager>();
            return manager.CreateServiceClient<T>(this.Credential, this.CancellationToken);
        }

        /// <inheritdoc/>
        public T CreateServiceClientByName<T>(string serviceName) where T : IOpenStackServiceClient
        {
            return this.CreateServiceClientByName<T>(serviceName, string.Empty);
        }

        /// <inheritdoc/>
        public T CreateServiceClientByName<T>(string serviceName, string version) where T : IOpenStackServiceClient
        {
            var manager = this.ServiceLocator.Locate<IOpenStackServiceClientManager>();
            return manager.CreateServiceClient<T>(this.Credential, serviceName, this.CancellationToken);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetSupportedVersions()
        {
            //TODO: Figure out the actual supported version, or a better way to handle cases where the client does not care about version.
            return new List<string> {"Any"};
        }

        /// <inheritdoc/>
        public bool IsSupported(ICredential credential, string version)
        {
            return credential is IOpenStackCredential;
        }
    }
}
