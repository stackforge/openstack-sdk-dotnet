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

using System;
using System.Collections.Generic;

namespace OpenStack.Compute
{
    /// <summary>
    /// Represents a compute server on a remote instance of OpenStack.
    /// </summary>
    public class ComputeServer : MetadataComputeItem
    {
        /// <summary>
        /// Gets the current status of the compute server.
        /// </summary>
        public ComputeServerStatus Status { get; internal set; }

        /// <summary>
        /// Gets the progress of the current server action.
        /// </summary>
        public int Progress { get; internal set; }

        /// <summary>
        /// Gets the administrative password for this server (only available after creating a new server).
        /// </summary>
        public string AdminPassword { get; internal set; }

        /// <summary>
        /// Creates a new instance of the ComputeServer class.
        /// </summary>
        /// <param name="id">The id of the compute server.</param>
        /// <param name="name">The name of the compute server.</param>
        /// <param name="publicUri">The public Uri for the compute server.</param>
        /// <param name="permanentUri">The permanent Uri for the computer server.</param>
        /// <param name="metadata">Metadata associated with the compute server.</param>
        internal ComputeServer(string id, string name, Uri publicUri, Uri permanentUri,
            IDictionary<string, string> metadata) : base(id, name, publicUri, permanentUri, metadata)
        {
            this.Status = ComputeServerStatus.Unknown;
        }

        /// <summary>
        /// Creates a new instance of the ComputeServer class.
        /// </summary>
        /// <param name="id">The id of the compute server.</param>
        /// <param name="name">The name of the compute server.</param>
        /// <param name="adminPassword">The administrative password for the compute instance.</param>
        /// <param name="publicUri">The public Uri for the compute server.</param>
        /// <param name="permanentUri">The permanent Uri for the computer server.</param>
        /// <param name="metadata">Metadata associated with the compute server.</param>
        internal ComputeServer(string id, string name, string adminPassword, Uri publicUri, Uri permanentUri,
            IDictionary<string, string> metadata)
            : base(id, name, publicUri, permanentUri, metadata)
        {
            this.Status = ComputeServerStatus.Unknown;
            this.AdminPassword = adminPassword;
        }

        /// <summary>
        /// Creates a new instance of the ComputeServer class.
        /// </summary>
        /// <param name="id">The id of the compute server.</param>
        /// <param name="name">The name of the compute server.</param>
        /// <param name="progress">The progress of the current action.</param>
        /// <param name="publicUri">The public Uri for the compute server.</param>
        /// <param name="permanentUri">The permanent Uri for the computer server.</param>
        /// <param name="metadata">Metadata associated with the compute server.</param>
        internal ComputeServer(string id, string name, ComputeServerStatus status, int progress, Uri publicUri, Uri permanentUri,
            IDictionary<string, string> metadata)
            : base(id, name, publicUri, permanentUri, metadata)
        {
            this.Status = status;
            this.Progress = progress;
        }
    }

}
