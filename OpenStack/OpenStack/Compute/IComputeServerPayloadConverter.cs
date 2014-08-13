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
    /// Converts a json payload into a compute server object.
    /// </summary>
    public interface IComputeServerPayloadConverter
    {
        /// <summary>
        /// Converts a json summary payload into a ComputeServer object.
        /// </summary>
        /// <param name="payload">The summary payload.</param>
        /// <returns>A ComputeServer object.</returns>
        ComputeServer ConvertSummary(string payload);

        /// <summary>
        /// Converts a json payload into a ComputeServer object.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>A ComputeServer object.</returns>
        ComputeServer Convert(string payload);

        /// <summary>
        /// Converts a json payload into a list of ComputeServer objects.
        /// </summary>
        /// <param name="payload">The payload to convert.</param>
        /// <returns>A list of ComputeServer objects.</returns>
        IEnumerable<ComputeServer> ConvertServers(string payload);
    }
}
