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
    using System.Threading.Tasks;
    using OpenStack.Common.Http;

    /// <summary>
    /// Client that can connect to the REST endpoints of an OpenStack Compute Service
    /// </summary>
    public interface IComputeServiceRestClient
    {
        /// <summary>
        /// Gets a list of compute Flavors from the remote OpenStack instance.
        /// </summary>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetFlavors();

        /// <summary>
        /// Gets the detailed metadata for a compute flavor.
        /// </summary>
        /// <param name="flavorId">The id of the flavor.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetFlavor(string flavorId);

        /// <summary>
        /// Gets a list of compute images from the remote OpenStack instance.
        /// </summary>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetImages();

        /// <summary>
        /// Gets the detailed info for a compute image.
        /// </summary>
        /// <param name="imageId">The id of the image.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> GetImage(string imageId);

        /// <summary>
        /// Deletes a compute image.
        /// </summary>
        /// <param name="imageId">The id of the image.</param>
        /// <returns>An HTTP response from the remote server.</returns>
        Task<IHttpResponseAbstraction> DeleteImage(string imageId);
    }
}
