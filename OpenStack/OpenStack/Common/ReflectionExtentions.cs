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
using System.Linq;
using System.Reflection;

namespace OpenStack.Common
{
    /// <summary>
    /// Static class for extending reflection classes.
    /// </summary>
    internal static class ReflectionExtentions
    {
        /// <summary>
        /// Gets the assembly that contains the extended type.
        /// </summary>
        /// <param name="input">The given Type</param>
        /// <returns>The assembly that contains the given type.</returns>
        public static Assembly GetAssembly(this Type input)
        {
            return input.Assembly;
        }

        /// <summary>
        /// Determines if the given type is an interface.
        /// </summary>
        /// <param name="input">The given type.</param>
        /// <returns>A value indicating if the given type is an interface.</returns>
        public static bool IsInterface(this Type input)
        {
            return input.IsInterface;
        }

        /// <summary>
        /// Gets a list of types that are defined in the given assembly.
        /// </summary>
        /// <param name="input">The given assembly.</param>
        /// <returns>A list of types defined in the given assembly.</returns>
        public static IEnumerable<Type> GetDefinedTypes(this Assembly input)
        {
            return input.GetTypes().ToList();
        }

        /// <summary>
        /// Gets a list of defined constructors for the given type.
        /// </summary>
        /// <param name="input">The given type.</param>
        /// <returns>A list of constructors for the given type.</returns>
        public static IEnumerable<ConstructorInfo> GetDefinedConstructors(this Type input)
        {
            return input.GetConstructors();
        }
    }
}
