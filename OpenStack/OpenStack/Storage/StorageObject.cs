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
using OpenStack.Common;

namespace OpenStack.Storage
{
    /// <summary>
    /// Represents a storage object.
    /// </summary>
    public class StorageObject : StorageItem
    {
        /// <summary>
        /// Gets the name of the parent storage container for the storage item.
        /// </summary>
        public string ContainerName { get; private set; }

        /// <summary>
        /// Gets the last modified data for the storage item.
        /// </summary>
        public DateTime LastModified { get; private set; }

        /// <summary>
        /// Gets the ETag for the storage item.
        /// </summary>
        public string ETag { get; private set; }

        /// <summary>
        /// Gets the length/size of the storage item.
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// Gets the metadata associated with the storage item.
        /// </summary>
        public IDictionary<string, string> Metadata { get; private set; }

        /// <summary>
        /// Creates a new instance of the StorageObject class.
        /// </summary>
        /// <param name="name">The name of the storage object.</param>
        /// <param name="containerName">The name of the parent storage container for the storage object.</param>
        /// <param name="contentType">The content type of the storage object.</param>
        /// <param name="metadata">The metadata associated with the storage object.</param>
        public StorageObject(string name, string containerName, string contentType, IDictionary<string, string> metadata)
            : this(name, containerName, DateTime.UtcNow, string.Empty, 0, contentType, metadata)
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageObject class.
        /// </summary>
        /// <param name="name">The name of the storage object.</param>
        /// <param name="containerName">The name of the parent storage container for the storage object.</param>
        public StorageObject(string name, string containerName)
            : this(name, containerName, DateTime.UtcNow, string.Empty, 0, string.Empty, new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageObject class.
        /// </summary>
        /// <param name="name">The name of the storage object.</param>
        /// <param name="containerName">The name of the parent storage container for the storage object.</param>
        /// <param name="metadata">The metadata associated with the storage object.</param>
        public StorageObject(string name, string containerName, IDictionary<string, string> metadata )
            : this(name, containerName, DateTime.UtcNow, string.Empty, 0, string.Empty, metadata)
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageObject class.
        /// </summary>
        /// <param name="name">The name of the storage object.</param>
        /// <param name="containerName">The name of the parent storage container for the storage object.</param>
        /// <param name="lastModified">The last modified data for the storage object.</param>
        /// <param name="eTag">The ETag for the storage object.</param>
        /// <param name="length">The length/size of the storage object.</param>
        /// <param name="contentType">The content type of the storage object.</param>
        internal StorageObject(string name, string containerName, DateTime lastModified, string eTag, long length, string contentType)
            : this(name, containerName, lastModified, eTag, length, contentType, new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageObject class.
        /// </summary>
        /// <param name="fullName">The full name of the storage object.</param>
        /// <param name="containerName">The name of the parent storage container for the storage object.</param>
        /// <param name="lastModified">The last modified data for the storage object.</param>
        /// <param name="eTag">The ETag for the storage object.</param>
        /// <param name="length">The length/size of the storage object.</param>
        /// <param name="contentType">The content type of the storage object.</param>
        /// <param name="metadata">The metadata associated with the storage object.</param>
        internal StorageObject(string fullName, string containerName, DateTime lastModified, string eTag, long length, string contentType, IDictionary<string, string> metadata) : base(fullName, contentType)
        {
            containerName.AssertIsNotNullOrEmpty("containerName");
            lastModified.AssertIsNotNull("lastModified");
            eTag.AssertIsNotNull("eTag");
            length.AssertIsNotNull("length");
            metadata.AssertIsNotNull("metadata");

            this.ContainerName = containerName;
            this.LastModified = lastModified;
            this.ETag = eTag;
            this.Length = length;
            this.Metadata = metadata;
        }
    }
}
