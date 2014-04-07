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

namespace OpenStack.Storage
{
    public interface IStorageFolderPayloadConverter
    {
        /// <summary>
        /// Converts a list of storage objects into a deep folder structure. 
        /// </summary>
        /// <param name="objects">The list of objects to convert.</param>
        /// <returns>A deep folder structure.</returns>
        IEnumerable<StorageFolder> Convert(IEnumerable<StorageObject> objects);

        /// <summary>
        /// Converts a Json payload into a shallow storage folder object.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="folderName">The full name of the folder.</param>
        /// <param name="payload">The Json payload.</param>
        /// <returns>A shallow storage folder object.</returns>
        StorageFolder Convert(string containerName, string folderName, string payload);
    }
}
