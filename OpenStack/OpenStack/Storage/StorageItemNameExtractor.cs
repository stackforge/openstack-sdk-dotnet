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

namespace OpenStack.Storage
{
    /// <summary>
    /// Extracts the "friendly" name of a storage item.
    /// </summary>
    public interface IStorageItemNameExtractor
    {
        /// <summary>
        /// Extracts the "friendly" name of a storage item, given the items full name.
        /// </summary>
        /// <param name="fullName">The items full name.</param>
        /// <returns>The items friendly name. (e.g. returns 'b' if the items full name is 'a/b')</returns>
        string ExtractName(string fullName);
    }

    /// <inheritdoc/>
    internal class StorageItemNameExtractor : IStorageItemNameExtractor
    {
        /// <inheritdoc/>
        public string ExtractName(string fullItemName)
        {
            var fullName = fullItemName.Trim('/');
            var lastIndex = fullName.LastIndexOf('/');
            lastIndex++;
            return fullName.Substring(lastIndex, fullName.Length - lastIndex);
        }
    }
}
