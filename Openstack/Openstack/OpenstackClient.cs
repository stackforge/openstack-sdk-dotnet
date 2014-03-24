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

using Openstack.Common;

namespace Openstack
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Openstack.Common.ServiceLocation;
    using Openstack.Identity;

    /// <inheritdoc/>
    public class OpenstackClient : IOpenstackClient
    {
        internal CancellationToken cancellationToken;

        /// <inheritdoc/>
        public IOpenstackCredential Credential { get; private set; }

        /// <summary>
        /// Creates a new instance of the OpenstackClient class.
        /// </summary>
        public OpenstackClient()
        {
            //TODO: remove the need for a default constructor, as state becomes an issue. This will need to be done in conjunction with changes in the ClientManager
        }

        /// <summary>
        ///  Creates a new instance of the OpenstackClient class.
        /// </summary>
        /// <param name="credential">The credential to be used by this client.</param>
        /// <param name="cancellationToken">The cancellation token to be used by this client.</param>
        public OpenstackClient(IOpenstackCredential credential, CancellationToken cancellationToken)
        {
            this.Credential = credential;
            this.cancellationToken = cancellationToken;
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
        public T CreateServiceClient<T>() where T : IOpenstackServiceClient
        {
            return this.CreateServiceClient<T>(string.Empty);
        }

        /// <inheritdoc/>
        public T CreateServiceClient<T>(string version) where T : IOpenstackServiceClient
        {
            var manager = ServiceLocator.Instance.Locate<IOpenstackServiceClientManager>();
            return manager.CreateServiceClient<T>(this.Credential, this.cancellationToken);
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
            return credential is IOpenstackCredential;
        }
    }
}
