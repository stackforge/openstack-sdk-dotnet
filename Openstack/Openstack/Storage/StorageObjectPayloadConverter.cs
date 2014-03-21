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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json.Linq;
    using Openstack.Common;
    using Openstack.Common.Http;

    /// <inheritdoc/>
    internal class StorageObjectPayloadConverter : IStorageObjectPayloadConverter
    {
        /// <inheritdoc/>
        public IEnumerable<StorageObject> Convert(string containerName, string payload)
        {
            payload.AssertIsNotNull("payload", "A null Storage Container payload cannot be converted.");

            var objects = new List<StorageObject>();

            if (String.IsNullOrEmpty(payload))
            {
                return objects;
            }

            try
            {
                var array = JArray.Parse(payload);
                objects.AddRange(array.Select(t => ConvertSingle(t, containerName)));
            }
            catch (HttpParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpParseException(string.Format("Storage Object payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return objects;
        }

        /// <summary>
        /// Converts a Json token into a storage object.
        /// </summary>
        /// <param name="obj">The token.</param>
        /// <param name="containerName">The name of the parent container.</param>
        /// <returns>The storage object.</returns>
        internal StorageObject ConvertSingle(JToken obj, string containerName)
        {
            string name = string.Empty;

            try
            {
                name = (string)obj["name"];
                var bytes = (long)obj["bytes"];
                var contentType = (string)obj["content_type"];
                var lastModified = (DateTime)obj["last_modified"];
                var etag = (string)obj["hash"];
                return new StorageObject(name, containerName, lastModified, etag, bytes, contentType);
            }
            catch (Exception ex)
            {
                var msg = "Storage Object payload could not be parsed.";
                if (!string.IsNullOrEmpty(name) && obj != null)
                {
                    msg = string.Format("Storage Object '{0}' payload could not be parsed. Payload: '{1}'", name, obj);
                }
                else if(obj != null)
                {
                    msg = string.Format("Storage Object payload could not be parsed. Payload: '{0}'", obj);
                }

                throw new HttpParseException(msg, ex);
            }
        }

        /// <inheritdoc/>
        public StorageObject Convert(string containerName, string objectName, IHttpHeadersAbstraction headers)
        {
            containerName.AssertIsNotNullOrEmpty("containerName");
            objectName.AssertIsNotNullOrEmpty("objectName");
            headers.AssertIsNotNull("headers");
           
            try
            {
                var lastModified = DateTime.Parse(headers["Last-Modified"].First());
                var eTag = headers["ETag"].First();
                var length = long.Parse(headers["Content-Length"].First());
                var contentType = headers["Content-Type"].First();
                var metadata = headers.Where(kvp => kvp.Key.StartsWith("X-Object-Meta")).ToDictionary(header => header.Key.Substring(14, header.Key.Length - 14), header => header.Value.First());

                return new StorageObject(objectName, containerName, lastModified, eTag, length, contentType, metadata);
            }
            catch (Exception ex)
            {
                throw new HttpParseException(string.Format("Storage object '{0}' could not be parsed.", objectName), ex);
            }
        }
    }
}
