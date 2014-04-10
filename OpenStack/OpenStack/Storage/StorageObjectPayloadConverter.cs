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

namespace OpenStack.Storage
{
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
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Storage Object payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return objects;
        }

        /// <inheritdoc/>
        public string Convert(IEnumerable<StorageObject> objects)
        {
            objects.AssertIsNotNull("objects","Cannot convert a null storage object collection to Json.");

            var objectsPayload = new JArray();
            foreach (var obj in objects)
            {
                dynamic item = new System.Dynamic.ExpandoObject();
                item.path = string.Format("{0}/{1}",obj.ContainerName, obj.FullName);
                item.size_bytes = obj.Length;
                item.etag = obj.ETag;

                objectsPayload.Add(JToken.FromObject(item));
            }

            return objectsPayload.ToString();
        }

        /// <inheritdoc/>
        public StorageObject ConvertSingle(JToken obj, string containerName)
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

                throw new FormatException(msg, ex);
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

                return CreateStorageObject(objectName, containerName, lastModified, eTag, length, contentType, metadata, headers);
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Storage object '{0}' could not be parsed.", objectName), ex);
            }
        }

        /// <summary>
        /// Creates the appropriate storage object based on the given parameters.
        /// </summary>
        /// <param name="objectName">the name of the object.</param>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="lastModified">The last modified date of the object.</param>
        /// <param name="eTag">The eTag for the object.</param>
        /// <param name="length">The length of the object.</param>
        /// <param name="contentType">The content type of the object.</param>
        /// <param name="metadata">The associated metadata for the object.</param>
        /// <param name="headers">The http headers from the object request.</param>
        /// <returns>An instance of the StorageObject class.</returns>
        internal StorageObject CreateStorageObject(string objectName, string containerName, DateTime lastModified, string eTag, long length, string contentType, IDictionary<string, string> metadata, IHttpHeadersAbstraction headers)
        {
            IEnumerable<string> values;
            if (headers.TryGetValue("X-Static-Large-Object", out values))
            {
                if (values.Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)))
                {
                    return new StaticLargeObjectManifest(objectName, containerName, lastModified, eTag, length, contentType, metadata);
                }
            }

            if (headers.TryGetValue("X-Object-Manifest", out values))
            {
                var segPath = values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
                if (!string.IsNullOrEmpty(segPath))
                {
                    return new DynamicLargeObjectManifest(objectName, containerName, lastModified, eTag, length, contentType, metadata, segPath);
                }
            }

            return new StorageObject(objectName, containerName, lastModified, eTag, length, contentType, metadata);
        }
    }
}
