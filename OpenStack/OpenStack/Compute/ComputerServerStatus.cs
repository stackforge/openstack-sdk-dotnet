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

namespace OpenStack.Compute
{
    /// <summary>
    /// Represents the states that a Compute Server can be in.
    /// </summary>
    public enum ComputeServerStatus
    {
        /// <summary>
        /// The compute server is active and available.
        /// </summary>
        Active,

        /// <summary>
        /// The compute server is down and unavailable.
        /// </summary>
        Build,

        /// <summary>
        /// The compute server has been deleted.
        /// </summary>
        Deleted,

        /// <summary>
        /// The compute server is in an error state.
        /// </summary>
        Error,

        /// <summary>
        /// The compute server is being hard rebooted.
        /// </summary>
        Hard_Reboot,

        /// <summary>
        /// The compute server is having its password reset.
        /// </summary>
        Password,

        /// <summary>
        /// The compute server is being soft rebooted.
        /// </summary>
        Reboot,

        /// <summary>
        /// The compute server is being rebuilt.
        /// </summary>
        Rebuild,

        /// <summary>
        /// The compute server in rescue mode.
        /// </summary>
        Rescue,

        /// <summary>
        /// The compute server being resized.
        /// </summary>
        Revert_Resize,

        /// <summary>
        /// The compute server is shut off.
        /// </summary>
        Shutoff,

        /// <summary>
        /// The compute server is suspended.
        /// </summary>
        Suspended,

        /// <summary>
        /// The compute server is awaiting verification that a resize operation was successful.
        /// </summary>
        Verify_Resize,

        /// <summary>
        /// The compute server is in an unknown state.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Static class for holding ComputeServer related extension methods.
    /// </summary>
    public static class ComputeServerStatusExtentions
    {
        /// <summary>
        /// Creates a ComputeServerStatus enum from a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A ComputeServerStatus enum.</returns>
        public static ComputeServerStatus GetComputeServerStatus(this string input)
        {
            input.AssertIsNotNullOrEmpty("input", "Cannot get computer server status with null or empty value.");

            switch (input.ToLowerInvariant())
            {
                case "active":
                    return ComputeServerStatus.Active;
                case "build":
                    return ComputeServerStatus.Build;
                case "deleted":
                    return ComputeServerStatus.Deleted;
                case "error":
                    return ComputeServerStatus.Error;
                case "hard_reboot":
                    return ComputeServerStatus.Hard_Reboot;
                case "password":
                    return ComputeServerStatus.Password;
                case "reboot":
                    return ComputeServerStatus.Reboot;
                case "rebuild":
                    return ComputeServerStatus.Rebuild;
                case "rescue":
                    return ComputeServerStatus.Rescue;
                case "revert_resize":
                    return ComputeServerStatus.Revert_Resize;
                case "shutoff":
                    return ComputeServerStatus.Shutoff;
                case "suspended":
                    return ComputeServerStatus.Suspended;
                case "verify_resize":
                    return ComputeServerStatus.Verify_Resize;
                default:
                    return ComputeServerStatus.Unknown;
            }
        }
    }
}
