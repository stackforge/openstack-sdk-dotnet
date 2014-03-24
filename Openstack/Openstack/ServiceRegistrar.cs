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
    using Openstack.Common.ServiceLocation;
    using Openstack.Identity;
    using Openstack.Storage;

    /// <inheritdoc/>
    public class ServiceRegistrar : IServiceLocationRegistrar
    {
        /// <inheritdoc/>
        public void Register(IServiceLocationManager manager, IServiceLocator locator)
        {
            //Storage related clients/services
            manager.RegisterServiceInstance(typeof(IStorageServicePocoClientFactory), new StorageServicePocoClientFactory());
            manager.RegisterServiceInstance(typeof(IStorageServiceRestClientFactory), new StorageServiceRestClientFactory());
            manager.RegisterServiceInstance(typeof(IStorageContainerNameValidator), new StorageContainerNameValidator());

            //Identity related clients/services
            manager.RegisterServiceInstance(typeof(IIdentityServicePocoClientFactory), new IdentityServicePocoClientFactory());
            manager.RegisterServiceInstance(typeof(IIdentityServiceRestClientFactory), new IdentityServiceRestClientFactory());
            manager.RegisterServiceInstance(typeof(IOpenstackServiceEndpointResolver), new OpenstackServiceEndpointResolver());
            manager.RegisterServiceInstance(typeof(IOpenstackRegionResolver), new OpenstackRegionResolver());

            //Converters
            manager.RegisterServiceInstance(typeof(IStorageContainerPayloadConverter), new StorageContainerPayloadConverter());
            manager.RegisterServiceInstance(typeof(IStorageObjectPayloadConverter), new StorageObjectPayloadConverter());
            manager.RegisterServiceInstance(typeof(IStorageAccountPayloadConverter), new StorageAccountPayloadConverter());
            manager.RegisterServiceInstance(typeof(IAccessTokenPayloadConverter), new AccessTokenPayloadConverter());
            manager.RegisterServiceInstance(typeof(IOpenstackServiceCatalogPayloadConverter), new OpenstackServiceCatalogPayloadConverter());
            manager.RegisterServiceInstance(typeof(IOpenstackServiceDefinitionPayloadConverter), new OpenstackServiceDefinitionPayloadConverter());
            manager.RegisterServiceInstance(typeof(IOpenstackServiceEndpointPayloadConverter), new OpenstackServiceEndpointPayloadConverter());
            

            //Client Management
            var clientManager = new OpenstackClientManager();
            clientManager.RegisterClient<OpenstackClient>();
            manager.RegisterServiceInstance(typeof(IOpenstackClientManager), clientManager);

            //Service Management
            var serviceManager = new OpenstackServiceClientManager();
            serviceManager.RegisterServiceClient<StorageServiceClient>(new StorageServiceClientDefinition());
            serviceManager.RegisterServiceClient<IdentityServiceClient>(new IdentityServiceClientDefinition());
            manager.RegisterServiceInstance(typeof(IOpenstackServiceClientManager), serviceManager);
        }
    }
}
