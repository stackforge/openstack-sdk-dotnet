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

using OpenStack.Common;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Storage
{
    public abstract class StorageItem
    {
        /// <summary>
        /// The "friendly" name of the folder
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The full name/path of the folder.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the content type of the storage item.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Creates a new instance of the StorageItem class.
        /// </summary>
        /// <param name="fullName">The full name of the item.</param>
        /// <param name="contentType">The content type of the item.</param>
        internal StorageItem(string fullName, string contentType)
        {
            fullName.AssertIsNotNullOrEmpty("fullName");
            contentType.AssertIsNotNull("contentType");

            this.FullName = fullName;
            this.Name = ExtractName(fullName);
            this.ContentType = contentType;
        }

        /// <summary>
        /// Extracts the "friendly" name from the items full name.
        /// </summary>
        /// <param name="fullItemName">The full name of the item.</param>
        /// <returns>The "friendly" name of the item. (e.g. "b" if the items full name is "a/b")</returns>
        internal static string ExtractName(string fullItemName)
        {
            var fullName = fullItemName.Trim('/');
            var lastIndex = fullName.LastIndexOf('/');
            lastIndex++;
            return fullName.Substring(lastIndex, fullName.Length - lastIndex);
        }
    }
}
