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

namespace OpenStack.Network
{
    /// <summary>
    /// Converter that can be used to convert an HTTP payload into a FloatingIp Poco object.
    /// </summary>
    public interface IFloatingIpPayloadConverter
    {
        /// <summary>
        /// Converts an HTTP payload into a FloatingIp object.
        /// </summary>
        /// <param name="payload">The HTTP payload to convert. </param>
        /// <returns>A FloatingIp object.</returns>
        FloatingIp Convert(string payload);

        /// <summary>
        /// Converts an HTTP payload into a list of FloatingIp objects.
        /// </summary>
        /// <param name="payload">The HTTP payload to convert.</param>
        /// <returns>An enumerable list of FloatingIp objects.</returns>
        IEnumerable<FloatingIp> ConvertFloatingIps(string payload);
    }
}
