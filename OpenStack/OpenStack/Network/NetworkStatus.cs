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

using OpenStack.Common;

namespace OpenStack.Network
{
    /// <summary>
    /// Represents the states that a Network can be in.
    /// </summary>
    public enum NetworkStatus
    {
        /// <summary>
        /// The network is active and available.
        /// </summary>
        Active, 

        /// <summary>
        /// The network is down and unavailable.
        /// </summary>
        Down, 

        /// <summary>
        /// The network is in an unknown state.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Static class for holding NetworkStatus related extention methods.
    /// </summary>
    public static class NetworkStatusExtentions
    {
        /// <summary>
        /// Creates a NetworkStatus enum from a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A NetworkStatus enum.</returns>
        public static NetworkStatus GetNetworkStatus(this string input)
        {
            input.AssertIsNotNullOrEmpty("input", "Cannot get network status with null or empty value.");

            switch (input.ToLowerInvariant())
            {
                case "active":
                    return NetworkStatus.Active;
                case "down":
                    return NetworkStatus.Down;
                default:
                    return NetworkStatus.Unknown;
            }
        }
    }
}
