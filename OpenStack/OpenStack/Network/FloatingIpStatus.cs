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
    /// Represents the states that a Floating IP can be in.
    /// </summary>
    public enum FloatingIpStatus
    {
        /// <summary>
        /// The Floating IP is active and available.
        /// </summary>
        Active, 

        /// <summary>
        /// The Floating IP is down and unavailable.
        /// </summary>
        Down, 

        /// <summary>
        /// The Floating IP is in an unknown state.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Static class for holding FloatingIpStatus related extention methods.
    /// </summary>
    public static class FloatingIpStatusExtentions
    {
        /// <summary>
        /// Creates a FloatingIpStatus enum from a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A FloatingIpStatus enum.</returns>
        public static FloatingIpStatus GetFloatingIpStatus(this string input)
        {
            input.AssertIsNotNullOrEmpty("input", "Cannot get Floating IP status with null or empty value.");

            switch (input.ToLowerInvariant())
            {
                case "active":
                    return FloatingIpStatus.Active;
                case "down":
                    return FloatingIpStatus.Down;
                default:
                    return FloatingIpStatus.Unknown;
            }
        }
    }
}
