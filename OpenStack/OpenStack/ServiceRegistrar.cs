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

using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;
using OpenStack.Identity;
using OpenStack.Network;
using OpenStack.Storage;

namespace OpenStack
{
    /// <inheritdoc/>
    public class ServiceRegistrar : IServiceLocationRegistrar
    {
        /// <inheritdoc/>
        public void Register(IServiceLocationManager manager, IServiceLocator locator)
        {
            //Common
            manager.RegisterServiceInstance(typeof(IHttpAbstractionClientFactory), new HttpAbstractionClientFactory());

            //Storage related clients/services
            manager.RegisterServiceInstance(typeof(IStorageServicePocoClientFactory), new StorageServicePocoClientFactory());
            manager.RegisterServiceInstance(typeof(IStorageServiceRestClientFactory), new StorageServiceRestClientFactory());
            manager.RegisterServiceInstance(typeof(IStorageContainerNameValidator), new StorageContainerNameValidator());
            manager.RegisterServiceInstance(typeof(IStorageFolderNameValidator), new StorageFolderNameValidator());
            manager.RegisterServiceInstance(typeof(ILargeStorageObjectCreatorFactory), new LargeStorageObjectCreatorFactory());

            //Compute related clients/services
            manager.RegisterServiceInstance(typeof(IComputeServicePocoClientFactory), new ComputeServicePocoClientFactory());
            manager.RegisterServiceInstance(typeof(IComputeServiceRestClientFactory), new ComputeServiceRestClientFactory());

            //Network related clients/services
            manager.RegisterServiceInstance(typeof(INetworkServicePocoClientFactory), new NetworkServicePocoClientFactory());
            manager.RegisterServiceInstance(typeof(INetworkServiceRestClientFactory), new NetworkServiceRestClientFactory());

            //Identity related clients/services
            manager.RegisterServiceInstance(typeof(IIdentityServicePocoClientFactory), new IdentityServicePocoClientFactory());
            manager.RegisterServiceInstance(typeof(IIdentityServiceRestClientFactory), new IdentityServiceRestClientFactory());
            manager.RegisterServiceInstance(typeof(IOpenStackRegionResolver), new OpenStackRegionResolver());

            //Converters
            manager.RegisterServiceInstance(typeof(IStorageContainerPayloadConverter), new StorageContainerPayloadConverter(locator));
            manager.RegisterServiceInstance(typeof(IStorageObjectPayloadConverter), new StorageObjectPayloadConverter());
            manager.RegisterServiceInstance(typeof(IStorageFolderPayloadConverter), new StorageFolderPayloadConverter(locator));
            manager.RegisterServiceInstance(typeof(IStorageAccountPayloadConverter), new StorageAccountPayloadConverter(locator));
            manager.RegisterServiceInstance(typeof(IAccessTokenPayloadConverter), new AccessTokenPayloadConverter());
            manager.RegisterServiceInstance(typeof(IOpenStackServiceCatalogPayloadConverter), new OpenStackServiceCatalogPayloadConverter(locator));
            manager.RegisterServiceInstance(typeof(IOpenStackServiceDefinitionPayloadConverter), new OpenStackServiceDefinitionPayloadConverter(locator));
            manager.RegisterServiceInstance(typeof(IOpenStackServiceEndpointPayloadConverter), new OpenStackServiceEndpointPayloadConverter());
            manager.RegisterServiceInstance(typeof(IComputeFlavorPayloadConverter), new ComputeFlavorPayloadConverter());
            manager.RegisterServiceInstance(typeof(INetworkPayloadConverter), new NetworkPayloadConverter());
            manager.RegisterServiceInstance(typeof(IFloatingIpPayloadConverter), new FloatingIpPayloadConverter());
            manager.RegisterServiceInstance(typeof(IComputeImagePayloadConverter), new ComputeImagePayloadConverter());
            manager.RegisterServiceInstance(typeof(IComputeItemMetadataPayloadConverter), new ComputeItemMetadataPayloadConverter());
            manager.RegisterServiceInstance(typeof(IComputeServerPayloadConverter), new ComputeServerPayloadConverter());
            manager.RegisterServiceInstance(typeof(IComputeKeyPairPayloadConverter), new ComputeKeyPairPayloadConverter());

            //Client Management
            var clientManager = new OpenStackClientManager(locator);
            clientManager.RegisterClient<OpenStackClient>();
            manager.RegisterServiceInstance(typeof(IOpenStackClientManager), clientManager);

            //Service Management
            var serviceManager = new OpenStackServiceClientManager(locator);
            serviceManager.RegisterServiceClient<StorageServiceClient>(new StorageServiceClientDefinition());
            serviceManager.RegisterServiceClient<IdentityServiceClient>(new IdentityServiceClientDefinition());
            serviceManager.RegisterServiceClient<ComputeServiceClient>(new ComputeServiceClientDefinition());
            serviceManager.RegisterServiceClient<NetworkServiceClient>(new NetworkServiceClientDefinition());
            manager.RegisterServiceInstance(typeof(IOpenStackServiceClientManager), serviceManager);
        }
    }
}
