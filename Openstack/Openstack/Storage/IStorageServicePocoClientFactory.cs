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
namespace Openstack.Storage
{
    /// <summary>
    /// Constructs a client that can be used to interact with the POCO objects related to the Openstack storage service.
    /// </summary>
    public interface IStorageServicePocoClientFactory
    {
        /// <summary>
        /// Creates a client that can be used to interact with the Openstack storage service.
        /// </summary>
        /// <param name="context">A storage service context to be used by the client.</param>
        /// <returns>The client.</returns>
        IStorageServicePocoClient Create(StorageServiceClientContext context);
    }
}
