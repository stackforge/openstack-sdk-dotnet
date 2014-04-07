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

using System;

namespace OpenStack.Identity
{
    /// <summary>
    /// A credential to be used when communicating with an instance of OpenStack.
    /// </summary>
    public interface ICredential
    {
        /// <summary>
        /// Gets the authentication endpoint to be used for the current instance of OpenStack.
        /// </summary>
        Uri AuthenticationEndpoint { get; }

        /// <summary>
        /// Gets the access token to be used for the current instance of OpenStack.
        /// </summary>
        string AccessTokenId { get; }

        /// <summary>
        /// Gets the service catalog for the current instance of OpenStack.
        /// </summary>
        OpenStackServiceCatalog ServiceCatalog { get; }
    }
}
