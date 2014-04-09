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

using System.Collections.Generic;
using System.Reflection;

namespace OpenStack.Common.ServiceLocation
{
    /// <summary>
    /// Interface for scanning an assembly for service location registrars.
    /// </summary>
    internal interface IServiceLocationAssemblyScanner
    {
        /// <summary>
        /// Gets a value indicating if the scanner has new assemblies that can be scanned.
        /// </summary>
        bool HasNewAssemblies { get; }

        /// <summary>
        /// Gets an enumerable collection of service location registrars.
        /// </summary>
        /// <returns>enumerable collection of service location registrars</returns>
        IEnumerable<IServiceLocationRegistrar> GetRegistrars();

        /// <summary>
        /// Gets an enumeration of service registrars that have not been scanned before.
        /// </summary>
        /// <returns>an enumeration of service registrars.</returns>
        IEnumerable<IServiceLocationRegistrar> GetNewRegistrars();

        /// <summary>
        /// Adds the target assembly to the scanners list of assemblies to scan.
        /// </summary>
        /// <param name="target">The target assembly.</param>
        void AddAssembly(Assembly target);
    }
}
