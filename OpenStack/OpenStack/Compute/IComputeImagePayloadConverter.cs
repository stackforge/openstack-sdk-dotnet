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
    /// <summary>
    /// Converter that can be used to convert an HTTP payload into a ComputeImage Poco object.
    /// </summary>
    public interface IComputeImagePayloadConverter
    {
        /// <summary>
        /// Converts an HTTP payload into a ComputeImage object.
        /// </summary>
        /// <param name="payload">The HTTP payload to convert. </param>
        /// <returns>A ComputeImage object.</returns>
        ComputeImage ConvertImage(string payload);

        /// <summary>
        /// Converts an HTTP payload into a list of ComputeImage objects.
        /// </summary>
        /// <param name="payload">The HTTP payload to convert.</param>
        /// <returns>An enumerable list of ComputeImage objects.</returns>
        IEnumerable<ComputeImage> ConvertImages(string payload);
    }
}
