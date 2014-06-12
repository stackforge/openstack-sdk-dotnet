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
    public abstract class MetadataComputeItem : ComputeItem
    {
        /// <summary>
        /// Gets a collection of metadata about this item.
        /// </summary>
        public IDictionary<string, string> Metadata { get; internal set; }

        /// <summary>
        /// Create a new instance of the ComputeItem class.
        /// </summary>
        /// <param name="id">The Id of the item.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="publicUri">The public Uri for the item.</param>
        /// <param name="permanentUri">The permanent Uri of the item.</param>
        /// <param name="metadata">Metadata associated with this item.</param>
        protected MetadataComputeItem(string id, string name, Uri publicUri, Uri permanentUri,
            IDictionary<string, string> metadata) : base(id, name, publicUri, permanentUri)
        {
            this.Metadata = metadata;
        }
    }
}
