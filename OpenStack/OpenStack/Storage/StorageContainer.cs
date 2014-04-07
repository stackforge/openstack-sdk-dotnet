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
using System.Linq;
using OpenStack.Common;

namespace OpenStack.Storage
{
    /// <summary>
    /// Represents a storage container
    /// </summary>
    public class StorageContainer
    {
        /// <summary>
        /// Gets the name of the storage container.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the total number of bytes used in the container.
        /// </summary>
        public long TotalBytesUsed { get; internal set; }

        /// <summary>
        /// Gets the total number of objects in the container.
        /// </summary>
        public int TotalObjectCount { get; internal set; }

        /// <summary>
        /// Gets a list of storage objects that are in the container.
        /// </summary>
        public IEnumerable<StorageObject> Objects { get; internal set; }

        /// <summary>
        /// Gets a list of storage folders that are in the container.
        /// </summary>
        public IEnumerable<StorageFolder> Folders { get; internal set; }

        /// <summary>
        /// Gets the metadata associated with the storage container.
        /// </summary>
        public IDictionary<string, string> Metadata { get; internal set; }

        /// <summary>
        /// Creates a new instance of the StorageContainer class.
        /// </summary>
        /// <param name="name">The name of the storage container.</param>
        /// <param name="totalBytes">The total number of bytes used in the container.</param>
        /// <param name="totalObjects">The total number of objects in the container.</param>
        /// <param name="objects">A list of storage objects that are in the container.</param>
        internal StorageContainer(string name, long totalBytes, int totalObjects, IEnumerable<StorageObject> objects) : this(name, totalBytes, totalObjects, new Dictionary<string, string>(), objects)
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageContainer class.
        /// </summary>
        /// <param name="name">The name of the storage container.</param>
        /// <param name="metadata">Metadata associated with the storage container.</param>
        public StorageContainer(string name, IDictionary<string,string> metadata) : this(name, 0, 0, metadata, new List<StorageObject>())
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageContainer class.
        /// </summary>
        /// <param name="name">The name of the storage container.</param>
        /// <param name="totalBytes">The total number of bytes used in the container.</param>
        /// <param name="totalObjects">The total number of objects in the container.</param>
        /// <param name="objects">A list of storage objects that are in the container.</param>
        /// <param name="metadata">Metadata associated with the storage container.</param>
        internal StorageContainer(string name, long totalBytes, int totalObjects, IDictionary<string,string> metadata, IEnumerable<StorageObject> objects) 
            : this(name, totalBytes,totalObjects, metadata, objects, new List<StorageFolder>())
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageContainer class.
        /// </summary>
        /// <param name="name">The name of the storage container.</param>
        /// <param name="totalBytes">The total number of bytes used in the container.</param>
        /// <param name="totalObjects">The total number of objects in the container.</param>
        /// <param name="objects">A list of storage objects that are in the container.</param>
        /// <param name="folders">A list of storage folders that are in the container.</param>
        /// <param name="metadata">Metadata associated with the storage container.</param>
        internal StorageContainer(string name, long totalBytes, int totalObjects, IDictionary<string, string> metadata, IEnumerable<StorageObject> objects, IEnumerable<StorageFolder> folders)
        {
            name.AssertIsNotNullOrEmpty("name");
            totalBytes.AssertIsNotNull("totalBytes");
            totalObjects.AssertIsNotNull("totalObjects");
            metadata.AssertIsNotNull("metadata");
            objects.AssertIsNotNull("objects");
            folders.AssertIsNotNull("folders");

            this.Name = name;
            this.TotalBytesUsed = totalBytes;
            this.TotalObjectCount = totalObjects;
            this.Objects = objects.ToList();
            this.Folders = folders.ToList();
            this.Metadata = metadata;
        }
    }
}
