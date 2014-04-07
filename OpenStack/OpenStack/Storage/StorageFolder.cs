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
    /// Represents a storage folder on a remote OpenStack instance.
    /// </summary>
    public class StorageFolder : StorageItem
    {
        /// <summary>
        /// A collection of sub-folders inside this folder.
        /// </summary>
        public ICollection<StorageFolder> Folders { get; private set; }

        /// <summary>
        /// A collection of objects inside this folder.
        /// </summary>
        public ICollection<StorageObject> Objects { get; private set; }

        /// <summary>
        /// Creates a new instance of the StorageFolder class.
        /// </summary>
        /// <param name="fullName">The full name/path of the folder.</param>
        /// <param name="folders">A collection of sub-folders that are inside this folder.</param>
        public StorageFolder(string fullName, IEnumerable<StorageFolder> folders) : this(fullName, folders, new List<StorageObject>())
        { }

        /// <summary>
        /// Creates a new instance of the StorageFolder class.
        /// </summary>
        /// <param name="fullName">The full name/path of the folder.</param>
        /// <param name="folders">A collection of sub-folders that are inside this folder.</param>
        /// <param name="objects">A collection of objects that are inside this folder.</param>
        public StorageFolder(string fullName, IEnumerable<StorageFolder> folders, IEnumerable<StorageObject> objects ) : base(fullName, "application/directory")
        {
            folders.AssertIsNotNull("folders", "Cannot create a storage folder with a null folders collection.");
            objects.AssertIsNotNull("objects", "Cannot create a storage folder with a null objects collection.");

            this.Folders = folders.ToList();
            this.Objects = objects.ToList();
        }
    }
}
