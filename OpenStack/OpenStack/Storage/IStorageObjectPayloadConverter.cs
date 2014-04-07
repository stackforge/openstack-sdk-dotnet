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
using Newtonsoft.Json.Linq;
using OpenStack.Common.Http;

namespace OpenStack.Storage
{
    /// <summary>
    /// Converts a Json payload into a storage object.
    /// </summary>
    interface IStorageObjectPayloadConverter
    {
        /// <summary>
        /// Converts a Json payload into a list of storage objects.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="payload">The Json payload.</param>
        /// <returns>A list of storage objects.</returns>
        IEnumerable<StorageObject> Convert(string containerName, string payload);

        /// <summary>
        /// Converts a collection of Http headers into a storage object.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the storage object.</param>
        /// <param name="headers">The collection of headers</param>
        /// <returns>The storage object.</returns>
        StorageObject Convert(string containerName, string objectName, IHttpHeadersAbstraction headers);

        /// <summary>
        /// Converts a collection of StorageObjects into a Json payload.
        /// </summary>
        /// <param name="objects">A collection of StorageObjects to convert.</param>
        /// <returns>The Json payload.</returns>
        string Convert(IEnumerable<StorageObject> objects);

        /// <summary>
        /// Converts a Json token into a storage object.
        /// </summary>
        /// <param name="obj">The token.</param>
        /// <param name="containerName">The name of the parent container.</param>
        /// <returns>The storage object.</returns>
        StorageObject ConvertSingle(JToken obj, string containerName);
    }
}
