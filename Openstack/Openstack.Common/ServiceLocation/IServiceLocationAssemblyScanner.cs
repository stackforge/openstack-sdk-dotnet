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

namespace Openstack.Common.ServiceLocation
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for scanning an assembly for service location registrars.
    /// </summary>
    internal interface IServiceLocationAssemblyScanner
    {
        /// <summary>
        /// Determines if the scanner has detected assemblies that have not been scanned.
        /// </summary>
        /// <returns>A value indicating if new assemblies are present.</returns>
        bool HasNewAssemblies();

        /// <summary>
        /// Gets an enumerable collection of service location registrars.
        /// </summary>
        /// <returns>enumerable collection of service location registrars</returns>
        IEnumerable<IServiceLocationRegistrar> GetRegistrars();
    }
}
