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

using System;
using System.Collections.Generic;

namespace OpenStack.Storage
{
    /// <summary>
    /// Base class for storage manifest objects.
    /// </summary>
    public abstract class StorageManifest : StorageObject
    {
        /// <summary>
        /// Creates a new instance of the StorageManifest class.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestName">The name of the manifest.</param>
        internal StorageManifest(string containerName, string manifestName)
            : base(manifestName, containerName)
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageManifest class.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestName">The name of the manifest.</param>
        /// <param name="metadata">The related metadata for the manifest.</param>
        internal StorageManifest(string containerName, string manifestName,IDictionary<string, string> metadata )
            : base(manifestName, containerName, metadata)
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageManifest class.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="fullName">The name of the manifest.</param>
        /// <param name="length">The length of the manifest.</param>
        /// /// <param name="contentType">The content type of the manifest.</param>
        /// <param name="lastModified">The time that the last modification of the manifest was made.</param>
        /// <param name="eTag">The eTag for the manifest.</param>
        /// <param name="metadata">The related metadata for the manifest.</param>
        internal StorageManifest(string fullName, string containerName, DateTime lastModified, string eTag,
            long length, string contentType, IDictionary<string, string> metadata)
            : base(fullName, containerName, lastModified, eTag, length, contentType, metadata)
        {
            
        }
    }
}
