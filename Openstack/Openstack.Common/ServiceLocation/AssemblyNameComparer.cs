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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Openstack.Common.ServiceLocation
{
    /// <summary>
    /// Interface for comparing assembly names.
    /// </summary>
    internal class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName>
    {
        /// <inheritdoc/>
        public bool Equals(AssemblyName x, AssemblyName y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            if (x.Name.Equals(y.Name, StringComparison.Ordinal) &&
                x.Version.Equals(y.Version) &&
                x.CultureInfo.Equals(y.CultureInfo) &&
                (ReferenceEquals(x.KeyPair, y.KeyPair) ||
                 (x.KeyPair != null && y.KeyPair!= null &&
                  x.KeyPair.PublicKey.SequenceEqual(y.KeyPair.PublicKey))))
            {
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public int GetHashCode(AssemblyName obj)
        {
            if (obj != null)
            {
                return obj.GetHashCode();
            }
            return 0;
        }
    }
}
