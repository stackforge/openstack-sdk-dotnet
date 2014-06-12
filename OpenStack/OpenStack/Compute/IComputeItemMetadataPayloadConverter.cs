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

using System.Collections.Generic;

namespace OpenStack.Compute
{
    public interface IComputeItemMetadataPayloadConverter
    {
        /// <summary>
        /// Converts a JSON payload into a collection of key value pairs.
        /// </summary>
        /// <param name="payload">The JSON payload to be converted.</param>
        /// <returns>A collection of key value pairs.</returns>
        IDictionary<string, string> Convert(string payload);

        /// <summary>
        /// Converts the given collection of key value pairs into a JSON payload.
        /// </summary>
        /// <param name="metadata">The collection of key value pairs.</param>
        /// <returns>A JSON payload that represents the key value pairs.</returns>
        string Convert(IDictionary<string, string> metadata);
    }
}
