// /* ============================================================================
// Copyright 2014 Hewlett Packard
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

namespace Openstack.Identity
{
    using System;
    using Openstack.Common;

    /// <summary>
    /// Represents an endpoint of an Openstack service.
    /// </summary>
    public class OpenstackServiceEndpoint
    {
        /// <summary>
        /// Gets the public Uri of the endpoint.
        /// </summary>
        public string PublicUri { get; private set; }

        /// <summary>
        /// Gets the region of the endpoint.
        /// </summary>
        public string Region { get; private set; }

        /// <summary>
        /// Gets the version of the endpoint.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Gets the Uri for the endpoints version information.
        /// </summary>
        public Uri VersionInformation { get; private set; }

        /// <summary>
        /// Gets the Uri for the endpoints list of versions.
        /// </summary>
        public Uri VersionList { get; private set; }

        /// <summary>
        /// Creates a new instance of the OpenstackServiceEndpoint class.
        /// </summary>
        /// <param name="publicUri">The public Uri of the endpoint.</param>
        /// <param name="region">The region of the endpoint.</param>
        /// <param name="version">The version of the endpoint.</param>
        /// <param name="versionInfo">The link to version information.</param>
        /// <param name="versionList">The link to a list of versions.</param>
        internal OpenstackServiceEndpoint(string publicUri, string region, string version, Uri versionInfo, Uri versionList)
        {
            publicUri.AssertIsNotNull("publicUri", "Cannot create a service endpoint with a null public URI.");
            region.AssertIsNotNull("region", "Cannot create a service endpoint with a null public URI.");
            version.AssertIsNotNull("version", "Cannot create a service endpoint with a null version.");
            versionInfo.AssertIsNotNull("versionInfo", "Cannot create a service endpoint with a null version information URI.");
            versionList.AssertIsNotNull("versionList", "Cannot create a service endpoint with a null version list URI.");

            this.PublicUri = publicUri;
            this.Region = region;
            this.Version = version;
            this.VersionInformation = versionInfo;
            this.VersionList = versionList;
        }
    }
}
