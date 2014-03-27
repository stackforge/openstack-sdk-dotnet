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

namespace Openstack.Storage
{
    using System.Linq;
    using Openstack.Common;
    using System.Collections.Generic;

    public class StorageFolder
    {
        public string Name { get; private set; }

        public string FullName { get; private set; }

        public ICollection<StorageFolder> Folders { get; private set; }

        public ICollection<StorageObject> Objects { get; private set; }

        public StorageFolder(string fullName, IEnumerable<StorageFolder> folders) : this(fullName, folders, new List<StorageObject>())
        { }

        public StorageFolder(string fullName, IEnumerable<StorageFolder> folders, IEnumerable<StorageObject> objects )
        {
            fullName.AssertIsNotNullOrEmpty("fullName", "Cannot create a storage folder with a null or empty full name.");
            folders.AssertIsNotNull("folders", "Cannot create a storage folder with a null folders collection.");
            objects.AssertIsNotNull("objects", "Cannot create a storage folder with a null objects collection.");

            this.FullName = fullName.Trim('/');
            this.Name = ExtractFolderName(this.FullName);
            this.Folders = folders.ToList();
            this.Objects = objects.ToList();
        }

        internal static string ExtractFolderName(string fullFolderName)
        {
            var fullName = fullFolderName.Trim('/');
            var lastIndex = fullName.LastIndexOf('/');
            lastIndex++;
            return fullName.Substring(lastIndex, fullName.Length - lastIndex);
        }
    }
}
