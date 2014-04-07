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

using System.Text.RegularExpressions;

namespace OpenStack.Storage
{
    /// <summary>
    /// Validates a container name.
    /// </summary>
    public interface IStorageFolderNameValidator
    {
        /// <summary>
        /// Validates a folder name.
        /// </summary>
        /// <param name="folderName">The name to validate.</param>
        /// <returns>A value indicating if the the name could be validated.</returns>
        bool Validate(string folderName);
    }

    /// <inheritdoc/>
    internal class StorageFolderNameValidator : IStorageFolderNameValidator
    {
        /// <inheritdoc/>
        public bool Validate(string folderName)
        {
            //Folder names cannot have consecutive slashes in their names. 
            //This is not a swift limitation, but it's good practice, and helps simplify things in the rest of the client.
            return !Regex.IsMatch(folderName, @"/{2,}");
        }
    }
}
