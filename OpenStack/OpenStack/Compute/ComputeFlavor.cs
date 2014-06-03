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
    using System;

    /// <summary>
    /// Represents a Flavor on a remote OpenStack instance.
    /// </summary>
    public class ComputeFlavor
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
        /// Gets the amount of RAM for the Flavor.
        /// </summary>
        public string Ram { get; private set; }

        /// <summary>
        /// Gets the number of virtual CPUs of the Flavor.
        /// </summary>
        public string Vcpus { get; private set; }

        /// <summary>
        /// Gets the size of the disk of the Flavor.
        /// </summary>
        public string Disk { get; private set; }

        /// <summary>
        /// Gets the public Uri of the Flavor.
        /// </summary>
        public Uri PublicUri { get; private set; }

        /// <summary>
        /// Gets the permanent Uri of the Flavor.
        /// </summary>
        public Uri PermanentUri { get; private set; }

        /// <summary>
        /// Create a new instance of the Flavor class.
        /// </summary>
        /// <param name="id">The Id of the flavor.</param>
        /// <param name="name">The name of the flavor.</param>
        /// <param name="ram">The amount of ram of the flavor.</param>
        /// <param name="vcpus">The number of virtual CPUs of the Flavor.</param>
        /// <param name="disk">The size of the disk of the Flavor.</param>
        /// <param name="publicUri">The public Uri for the Flavor.</param>
        /// <param name="permanentUri">The permanent Uri of the Flavor.</param>
        internal ComputeFlavor(string id, string name, string ram, string vcpus, string disk, Uri publicUri, Uri permanentUri)
        {
            this.Id = id;
            this.Name = name;
            this.Ram = ram;
            this.Vcpus = vcpus;
            this.Disk = disk;
            this.PublicUri = publicUri;
            this.PermanentUri = permanentUri;
        }
    }
}
