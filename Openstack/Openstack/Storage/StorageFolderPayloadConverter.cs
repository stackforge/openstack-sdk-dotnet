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
    using System;
    using System.Linq;
    using Openstack.Common;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal class StorageFolderPayloadConverter : IStorageFolderPayloadConverter
    {
        internal const string consecutiveSlashRegex = @"/{2,}";

        public IEnumerable<StorageFolder> Convert(IEnumerable<StorageObject> objects)
        {
            objects.AssertIsNotNull("objects", "Cannot build folders with a null object collection.");
            
            var folders = new List<StorageFolder>();

            var sortedObjectList = objects.OrderByDescending(o => o.Name.Length).ToList();

            foreach (var obj in sortedObjectList)
            {
                //if the name has any consecutive slashes in the name, skip it.
                if (Regex.IsMatch(obj.Name, consecutiveSlashRegex))
                {
                    continue;
                }

                //split the input using a forward slash as the folder delimiter, and separate the object name (if we have one) and the folder path.
                var folderParts = obj.Name.TrimStart('/').Split('/');
                var objectName = folderParts.Last(); //this will be string.empty if the object name ends in a "/" indicating that it's a folder.
                folderParts = folderParts.Take(folderParts.Length - 1).ToArray();

                //if there are no folders in the object's name, skip it.
                if (folderParts.Count() <= 0)
                {
                    continue;
                }

                var currentRoot = folders.FirstOrDefault(f => string.Compare(f.Name, folderParts[0], StringComparison.InvariantCulture) == 0);
                if (currentRoot == null)
                {
                    //if the root folder does not exist, create it.
                    currentRoot = new StorageFolder(folderParts[0], new List<StorageFolder>());
                    folders.Add(currentRoot);
                }

                //go through the rest of the folder path (if any) and add nested folders (if needed).
                var currentPath = folderParts[0];
                foreach (var part in folderParts.Skip(1))
                {
                    currentPath += "/" + part;
                    var newRoot = currentRoot.Folders.FirstOrDefault(f => string.Compare(f.Name, part, StringComparison.InvariantCulture) == 0);
                    if (newRoot == null)
                    {
                        newRoot = new StorageFolder(currentPath, new List<StorageFolder>());
                        currentRoot.Folders.Add(newRoot);
                    }
                    currentRoot = newRoot;
                }

                //if this object is not a folder (e.g. the name does not end in a /), then add this object to the current folder.
                if (!string.IsNullOrEmpty(objectName))
                {
                    currentRoot.Objects.Add(obj);
                }
            }
            return folders;
        }
    }
}
