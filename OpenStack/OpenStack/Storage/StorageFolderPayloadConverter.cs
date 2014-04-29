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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Storage
{
    /// <inheritdoc/>
    internal class StorageFolderPayloadConverter : IStorageFolderPayloadConverter
    {
        internal const string consecutiveSlashRegex = @"/{2,}";
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the StorageFolderPayloadConverter class.
        /// </summary>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        public StorageFolderPayloadConverter(IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a storage folder payload converter with a null service locator.");
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public IEnumerable<StorageFolder> Convert(IEnumerable<StorageObject> objects)
        {
            objects.AssertIsNotNull("objects", "Cannot build folders with a null object collection.");
            
            var folders = new List<StorageFolder>();

            var sortedObjectList = objects.OrderByDescending(o => o.Name.Length).ToList();

            foreach (var obj in sortedObjectList)
            {
                //if the name has any consecutive slashes in the name, skip it.
                if (Regex.IsMatch(obj.FullName, consecutiveSlashRegex))
                {
                    continue;
                }

                //split the input using a forward slash as the folder delimiter, and separate the object name (if we have one) and the folder path.
                var folderParts = obj.FullName.TrimStart('/').Split('/');
                var objectName = folderParts.Last(); //this will be string.empty if the object name ends in a "/" indicating that it's a folder.
                folderParts = folderParts.Take(folderParts.Length - 1).ToArray();

                //if there are no folders in the object's name, skip it.
                if (folderParts.Count() <= 0)
                {
                    continue;
                }

                var currentRoot = folders.FirstOrDefault(f => string.Equals(f.Name, folderParts[0], StringComparison.Ordinal));
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
                    var newRoot = currentRoot.Folders.FirstOrDefault(f => string.Equals(f.Name, part, StringComparison.Ordinal));
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

        /// <inheritdoc/>
        public StorageFolder Convert(string containerName, string folderName, string payload)
        {
            if (String.IsNullOrEmpty(payload))
            {
                return new StorageFolder(folderName, new List<StorageFolder>());
            }

            try
            {
                var array = JArray.Parse(payload);
                if (array.Count == 0)
                {
                    throw new InvalidDataException("Folder cannot be converted. The folder does not exist, has no children, and cannot be inferred.");
                }

                var subFolders = array.Where(t => t["subdir"] != null);
                var rawObjects = array.Where(t => t["subdir"] == null);

                var objectConverter = this.ServiceLocator.Locate<IStorageObjectPayloadConverter>();

                var objects = rawObjects.Select(t => objectConverter.ConvertSingle(t,containerName)).ToList();
                objects.RemoveAll(o => string.Equals(o.FullName, folderName, StringComparison.Ordinal));

                return new StorageFolder(folderName, subFolders.Select(ParseSubFolder), objects);
                
            }
            catch (FormatException)
            {
                throw;
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Storage Container payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a JToken object into a StorageFolder object.
        /// </summary>
        /// <param name="token">The JToken to convert.</param>
        /// <returns>A StorageFolder object.</returns>
        internal StorageFolder ParseSubFolder(JToken token)
        {
            try
            {
                var fullName = (string) token["subdir"];
                return new StorageFolder(fullName, new List<StorageFolder>());
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Storage Folder payload could not be parsed. Payload: '{0}'", token), ex);
            }
        }
    }
}
