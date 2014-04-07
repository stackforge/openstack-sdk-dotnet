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
    /// Represents a storage account.
    /// </summary>
    public class StorageAccount
    {
        /// <summary>
        /// Gets the name of the storage account.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the total number of bytes used in the account.
        /// </summary>
        public long TotalBytesUsed { get; private set; }

        /// <summary>
        /// Gets the total number of storage objects in the account.
        /// </summary>
        public int TotalObjectCount { get; private set; }

        /// <summary>
        /// Gets the total number of storage containers in the account.
        /// </summary>
        public int TotalContainerCount { get; private set; }

        /// <summary>
        /// Gets a list of storage containers in the account.
        /// </summary>
        public IEnumerable<StorageContainer> Containers { get; private set; }

        /// <summary>
        /// Gets the metadata associated with the account.
        /// </summary>
        public IDictionary<string, string> Metadata { get; private set; }

        /// <summary>
        /// Creates a new instance of the StorageAccount class.
        /// </summary>
        /// <param name="name">The name of the account.</param>
        /// <param name="totalBytes">The total number of bytes used in the account.</param>
        /// <param name="totalObjects">The total number of storage objects in the account.</param>
        /// <param name="totalContainers">The total number of storage containers in the account.</param>
        /// <param name="containers">A list of storage containers in the account.</param>
        internal StorageAccount(string name, long totalBytes, int totalObjects, int totalContainers, IEnumerable<StorageContainer> containers)
            : this(name, totalBytes, totalObjects, totalContainers, new Dictionary<string, string>(), containers)
        {
        }

        /// <summary>
        /// Creates a new instance of the StorageAccount class.
        /// </summary>
        /// <param name="name">The name of the account.</param>
        /// <param name="totalBytes">The total number of bytes used in the account.</param>
        /// <param name="totalObjects">The total number of storage objects in the account.</param>
        /// <param name="totalContainers">The total number of storage containers in the account.</param>
        /// <param name="metadata">Metadata associated with this account.</param>
        /// <param name="containers">A list of storage containers in the account.</param>
        internal StorageAccount(string name, long totalBytes, int totalObjects, int totalContainers, IDictionary<string, string> metadata, IEnumerable<StorageContainer> containers)
        {
            name.AssertIsNotNullOrEmpty("name");
            totalBytes.AssertIsNotNull("totalBytes");
            totalObjects.AssertIsNotNull("totalObjects");
            metadata.AssertIsNotNull("metadata");
            containers.AssertIsNotNull("containers");

            this.Name = name;
            this.TotalBytesUsed = totalBytes;
            this.TotalObjectCount = totalObjects;
            this.TotalContainerCount = totalContainers;
            this.Containers = containers.ToList();
            this.Metadata = metadata;
        }
    }
}
