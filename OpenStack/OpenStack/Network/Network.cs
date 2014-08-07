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

namespace OpenStack.Network
{
    /// <summary>
    /// Represents a network on a remote OpenStack instance.
    /// </summary>
    public class Network
    {
        /// <summary>
        /// Gets the name of the Network.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the id of the Network.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the status of the Network.
        /// </summary>
        public NetworkStatus Status { get; internal set; }

        /// <summary>
        /// Create a new instance of the ComputeItem class.
        /// </summary>
        /// <param name="id">The Id of the network.</param>
        /// <param name="name">The name of the network.</param>
        /// <param name="status">The status of the network.</param>
        internal Network(string id, string name, NetworkStatus status)
        {
            this.Id = id;
            this.Name = name;
            this.Status = status;
        }
    }
}
