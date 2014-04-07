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
using OpenStack.Common;

namespace OpenStack.Storage
{
    /// <summary>
    /// Represents a manifest for a static large object on the remote OpenStack instance.
    /// </summary>
    public class StaticLargeObjectManifest : StorageManifest
    {
        /// <summary>
        /// Gets a collection of objects in the manifest.
        /// </summary>
        public ICollection<StorageObject> Objects { get; internal set; }
 
        /// <summary>
        /// Creates a new instance of the StaticLargeObjectManifest class.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestFullName">The full name of the manifest.</param>
        /// <param name="objects">A collection of objects that are included in the manifest.</param>
        public StaticLargeObjectManifest(string containerName, string manifestFullName, ICollection<StorageObject> objects ) : base(containerName, manifestFullName)
        {
            objects.AssertIsNotNull("objects","Cannot create a static large object manifest with a null object collection.");
            this.Objects = objects;
        }

        /// <summary>
        /// Creates a new instance of the StaticLargeObjectManifest class.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestFullName">The full name of the manifest.</param>
        /// <param name="objects">A collection of objects that are included in the manifest.</param>
        /// <param name="metadata">The metadata associated with the storage manifest.</param>
        public StaticLargeObjectManifest(string containerName, string manifestFullName, IDictionary<string, string> metadata, ICollection<StorageObject> objects)
            : base(containerName, manifestFullName, metadata)
        {
            objects.AssertIsNotNull("objects", "Cannot create a static large object manifest with a null object collection.");
            this.Objects = objects;
        }

        /// <summary>
        /// Creates a new instance of the StaticLargeObjectManifest class.
        /// </summary>
        /// <param name="fullName">The full name of the storage manifest.</param>
        /// <param name="containerName">The name of the parent storage container for the storage manifest.</param>
        /// <param name="lastModified">The last modified data for the storage manifest.</param>
        /// <param name="eTag">The ETag for the storage manifest.</param>
        /// <param name="length">The length/size of the storage manifest.</param>
        /// <param name="contentType">The content type of the storage manifest.</param>
        /// <param name="metadata">The metadata associated with the storage manifest.</param>
        internal StaticLargeObjectManifest(string fullName, string containerName, DateTime lastModified, string eTag,
            long length, string contentType, IDictionary<string, string> metadata)
            : base(fullName, containerName, lastModified, eTag, length, contentType, metadata)
        {
            this.Objects = new List<StorageObject>();
        }
    }
}
