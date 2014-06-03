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

namespace OpenStack.Compute
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Client that can interact with an OpenStack compute service.
    /// </summary>
    public interface IComputeServicePocoClient
    {
        /// <summary>
        /// Gets a list of flavors that are available on the remote OpenStack instance.
        /// </summary>
        /// <returns>An enumerable list of Flavors.</returns>
        Task<IEnumerable<ComputeFlavor>> GetFlavors();

        /// <summary>
        /// Gets the detailed metadata for a compute flavor.
        /// </summary>
        /// <param name="flavorId">The id of the flavor.</param>
        /// <returns>An object representing a compute flavor.</returns>
        Task<ComputeFlavor> GetFlavor(string flavorId);
    }
}
