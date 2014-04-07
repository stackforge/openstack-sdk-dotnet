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

using System.Threading.Tasks;
using OpenStack.Common.Http;

namespace OpenStack.Identity
{
    /// <summary>
    /// A client that can be used to create and interact with REST interfaces related to the OpenStack identity service.
    /// </summary>
    public interface IIdentityServiceRestClient
    {
        /// <summary>
        /// Authenticates against a remote OpenStack instance.
        /// </summary>
        /// <returns>A credential that can be used to interact with the remote instance of OpenStack.</returns>
        Task<IHttpResponseAbstraction> Authenticate();
    }
}
