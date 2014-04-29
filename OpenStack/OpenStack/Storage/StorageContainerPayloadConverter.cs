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
using System.Linq;
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using OpenStack.Common.Http;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Storage
{
    /// <inheritdoc/>
    internal class StorageContainerPayloadConverter : IStorageContainerPayloadConverter
    {
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the StorageContainerPayloadConverter class.
        /// </summary>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        public StorageContainerPayloadConverter(IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a storage container payload converter with a null service locator.");
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public IEnumerable<StorageContainer> Convert(string payload)
        {
            payload.AssertIsNotNull("payload", "A null Storage Container payload cannot be converted.");

            var containers = new List<StorageContainer>();

            if (String.IsNullOrEmpty(payload))
            {
                return containers;
            }

            try
            {
                var array = JArray.Parse(payload);
                containers.AddRange(array.Select(ConvertSingle));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Storage Container payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return containers;
        }

        /// <summary>
        /// Converts a Json token into a storage container.
        /// </summary>
        /// <param name="container">The token.</param>
        /// <returns>The storage container.</returns>
        internal StorageContainer ConvertSingle(JToken container)
        {
            string name = string.Empty;

            try
            {
                name = (string)container["name"];
                var bytes = (long)container["bytes"];
                var objectCount = (int)container["count"];
                return new StorageContainer(name,bytes, objectCount, new Dictionary<string, string>(), new List<StorageObject>());
            }
            catch (Exception ex)
            {
                var msg = "Storage container payload could not be parsed.";
                if (!string.IsNullOrEmpty(name) && container != null)
                {
                    msg = string.Format("Storage Container '{0}' payload could not be parsed. Payload: '{1}'", name, container);
                }
                else if (container != null)
                {
                    msg = string.Format("Storage Container payload could not be parsed. Payload: '{0}'", container);
                }

                throw new FormatException(msg, ex);
            }
        }

        /// <inheritdoc/>
        public StorageContainer Convert(string name, IHttpHeadersAbstraction headers, string payload)
        {
            name.AssertIsNotNullOrEmpty("name");
            headers.AssertIsNotNull("headers");
            payload.AssertIsNotNull("payload");

            var objectConverter = this.ServiceLocator.Locate<IStorageObjectPayloadConverter>();
            var folderConverter = this.ServiceLocator.Locate<IStorageFolderPayloadConverter>();

            try
            {
                var totalBytes = long.Parse(headers["X-Container-Bytes-Used"].First());
                var totalObjects = int.Parse(headers["X-Container-Object-Count"].First());
                var metadata = headers.Where(kvp => kvp.Key.StartsWith("X-Container-Meta")).ToDictionary(header => header.Key.Substring(17, header.Key.Length - 17), header => header.Value.First());
                var objects = objectConverter.Convert(name, payload);
                var folders = folderConverter.Convert(objects);

                return new StorageContainer(name, totalBytes, totalObjects, metadata, objects, folders);
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Storage Container '{0}' payload could not be parsed.", name), ex);
            }
        }
    }
}
