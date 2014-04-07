// /* ============================================================================
// Copyright 2014 Hewlett Packard
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenStack.Storage
{
    /// <summary>
    /// Creates large storage objects on a remote OpenStack instance.
    /// </summary>
    public interface ILargeStorageObjectCreator
    {
        /// <summary>
        /// Creates a large storage object on a remote OpenStack instance.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the object to create.</param>
        /// <param name="metadata">The metadata associated with the object.</param>
        /// <param name="content">The contents of the object.</param>
        /// <param name="numberOfSegments">The number of segments to use when creating the object.</param>
        /// <param name="segmentsContainer">The name of the container that will hold the object segments.</param>
        /// <returns>A StorageObject that represents the large object.</returns>
        Task<StorageObject> Create(string containerName, string objectName, IDictionary<string, string> metadata, Stream content, int numberOfSegments, string segmentsContainer);
    }
}
