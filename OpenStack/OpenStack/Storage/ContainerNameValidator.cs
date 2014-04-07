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

namespace OpenStack.Storage
{
    /// <summary>
    /// Validates a container name.
    /// </summary>
    public interface IStorageContainerNameValidator
    {
        /// <summary>
        /// Validates a container name.
        /// </summary>
        /// <param name="containerName">The name to validate.</param>
        /// <returns>A value indicating if the the name could be validated.</returns>
        bool Validate(string containerName);
    }

    /// <inheritdoc/>
    internal class StorageContainerNameValidator : IStorageContainerNameValidator
    {
        /// <inheritdoc/>
        public bool Validate(string containerName)
        {
            return !(containerName.Contains("/") || containerName.Contains("\\"));
        }
    }
}
