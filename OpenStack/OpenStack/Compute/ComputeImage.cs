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
    /// Represents an image on a remote OpenStack instance.
    /// </summary>
    public class ComputeImage : MetadataComputeItem
    {
        /// <summary>
        /// Gets the current status of the image.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Gets the date and time when the image was created.
        /// </summary>
        public DateTime CreateDate { get; private set; }

        /// <summary>
        /// Gets the date and time that the image was last updated.
        /// </summary>
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        /// Gets the minimum size of the disk for this image.
        /// </summary>
        public int MinimumDiskSize { get; private set; }

        /// <summary>
        /// Gets the current upload progress of the image.
        /// </summary>
        public int UploadProgress { get; private set; }

        /// <summary>
        /// Gets the minimum amount of ram for this image.
        /// </summary>
        public int MinimumRamSize { get; private set; }

        /// <summary>
        /// Create a new instance of the ComputeImage class.
        /// </summary>
        /// <param name="id">The Id of the image.</param>
        /// <param name="name">The name of the image.</param>
        /// <param name="publicUri">The public Uri for the image.</param>
        /// <param name="permanentUri">The permanent Uri of the image.</param>
        /// <param name="metadata">The metadata for the image.</param>
        /// <param name="status">The current status for the image.</param>
        /// <param name="created">The date and time when the image was created.</param>
        /// <param name="updated">The data and time when the image was last updated.</param>
        /// <param name="minDisk">The minimum disk size for the image.</param>
        /// <param name="minRam">The minimum ram size for the image.</param>
        /// <param name="progress">The upload progress for the image.</param>
        internal ComputeImage(string id, string name, Uri publicUri, Uri permanentUri,
            IDictionary<string, string> metadata, string status, DateTime created, DateTime updated, int minDisk,
            int minRam, int progress) : base(id, name, publicUri, permanentUri, metadata)
        {
            this.Status = status;
            this.CreateDate = created;
            this.LastUpdated = updated;
            this.MinimumDiskSize = minDisk;
            this.MinimumRamSize = minRam;
            this.UploadProgress = progress;
        }
    }
}
