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
    /// Represents the manifest for a dynamic large object on the remote instance of OpenStack.
    /// </summary>
    public class DynamicLargeObjectManifest : StorageManifest
    {
        /// <summary>
        /// Gets the path where the object segments that make up the manifest can be found.
        /// </summary>
        public string SegmentsPath { get; internal set; }

        /// <summary>
        /// Creates a new instance of the DynamicLargeObjectManifest class.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestFullName">The full name of the manifest file</param>
        /// <param name="segmentsPath">The path where the segments that make up this manifest can be found.</param>
        public DynamicLargeObjectManifest(string containerName, string manifestFullName, string segmentsPath) : this(containerName, manifestFullName, new Dictionary<string, string>(), segmentsPath)
        {
        }

        /// <summary>
        /// Creates a new instance of the DynamicLargeObjectManifest class.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestFullName">The full name of the manifest file</param>
        /// <param name="metadata">The metadata associated with the storage manifest.</param>
        /// <param name="segmentsPath">The path where the segments that make up this manifest can be found.</param>
        public DynamicLargeObjectManifest(string containerName, string manifestFullName, IDictionary<string,string> metadata, string segmentsPath)
            : base(containerName, manifestFullName, metadata)
        {
            segmentsPath.AssertIsNotNullOrEmpty("segmentsPath", "Cannot create a dynamic large object manifest with a null or empty segments path.");
            this.SegmentsPath = segmentsPath;
        }

        /// <summary>
        /// Creates a new instance of the DynamicLargeObjectManifest class.
        /// </summary>
        /// <param name="fullName">The full name of the storage manifest.</param>
        /// <param name="containerName">The name of the parent storage container for the storage manifest.</param>
        /// <param name="lastModified">The last modified data for the storage manifest.</param>
        /// <param name="eTag">The ETag for the storage manifest.</param>
        /// <param name="length">The length/size of the storage manifest.</param>
        /// <param name="contentType">The content type of the storage manifest.</param>
        /// <param name="metadata">The metadata associated with the storage manifest.</param>
        /// <param name="segmentsPath">The path where the segments that make up this manifest can be found.</param>
        internal DynamicLargeObjectManifest(string fullName, string containerName, DateTime lastModified, string eTag,
            long length, string contentType, IDictionary<string, string> metadata, string segmentsPath)
            : base(fullName, containerName, lastModified, eTag, length, contentType, metadata)
        {
            this.SegmentsPath = segmentsPath;
        }
    }
}
