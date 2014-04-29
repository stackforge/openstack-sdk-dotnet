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
using System.Linq;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Storage
{
    /// <inheritdoc/>
    internal class StorageAccountPayloadConverter : IStorageAccountPayloadConverter
    {
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the StorageAccountPayloadConverter class.
        /// </summary>
        /// <param name="serviceLocator">A service locator that will be used to locate dependent services.</param>
        public StorageAccountPayloadConverter(IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a storage account payload converter with a null service locator.");
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public StorageAccount Convert(string name, IHttpHeadersAbstraction headers, string payload)
        {
            name.AssertIsNotNullOrEmpty("name");
            headers.AssertIsNotNull("headers");
            payload.AssertIsNotNull("payload");

            var containerConverter = this.ServiceLocator.Locate<IStorageContainerPayloadConverter>();

            try
            {
                var totalBytes = long.Parse(headers["X-Account-Bytes-Used"].First());
                var totalObjects = int.Parse(headers["X-Account-Object-Count"].First());
                var totalContainers = int.Parse(headers["X-Account-Container-Count"].First());
                var containers = containerConverter.Convert(payload);
                var metadata = headers.Where(kvp => kvp.Key.StartsWith("X-Account-Meta")).ToDictionary(header => header.Key.Substring(15, header.Key.Length - 15), header => header.Value.First());

                return new StorageAccount(name, totalBytes, totalObjects, totalContainers, metadata, containers);
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Storage Account '{0}' payload could not be parsed.", name), ex);
            }
        }
    }
}
