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
    /// Represents a Floating IP address on the remote OpenStack instance.
    /// </summary>
    public class FloatingIp
    {
        /// <summary>
        /// Gets the id of the FloatingIp.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the status of the FloatingIp.
        /// </summary>
        public FloatingIpStatus Status { get; internal set; }

        /// <summary>
        /// Gets the floating ip address of the FloatingIp.
        /// </summary>
        public string FloatingIpAddress { get; internal set; }

        /// <summary>
        /// Create a new instance of the FloatingIp class.
        /// </summary>
        /// <param name="id">The Id of the floating ip.</param>
        /// <param name="FloatingIpAddress">The floating ip address of the floating ip.</param>
        /// <param name="status">The status of the floating ip.</param>
        internal FloatingIp(string id, string FloatingIpAddress, FloatingIpStatus status)
        {
            this.Id = id;
            this.FloatingIpAddress = FloatingIpAddress;
            this.Status = status;
        }
    }
}
