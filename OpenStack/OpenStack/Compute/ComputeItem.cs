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

namespace OpenStack.Compute
{
    public abstract class ComputeItem
    {
        /// <summary>
        /// Gets the name of the Flavor.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the id of the Flavor.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the public Uri of the Flavor.
        /// </summary>
        public Uri PublicUri { get; private set; }

        /// <summary>
        /// Gets the permanent Uri of the Flavor.
        /// </summary>
        public Uri PermanentUri { get; private set; }

        /// <summary>
        /// Create a new instance of the ComputeItem class.
        /// </summary>
        /// <param name="id">The Id of the item.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="publicUri">The public Uri for the item.</param>
        /// <param name="permanentUri">The permanent Uri of the item.</param>
        protected ComputeItem(string id, string name, Uri publicUri, Uri permanentUri)
        {
            this.Id = id;
            this.Name = name;
            this.PublicUri = publicUri;
            this.PermanentUri = permanentUri;
        }
    }
}
