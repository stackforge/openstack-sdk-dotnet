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

namespace OpenStack.Common.ServiceLocation
{
    /// <inheritdoc/>
    internal class ServiceLocationAssemblyScanner : IServiceLocationAssemblyScanner
    {
        internal IServiceLocationRegistrarFactory ServiceRegistrarFactory { get; set; }
        internal readonly List<Type> _registrars = new List<Type>();
        internal readonly List<Assembly> _assemblies = new List<Assembly>();
        internal Func<IEnumerable<Type>> GetRegistrarTypes; 

        /// <inheritdoc/>
        public bool HasNewAssemblies { get; internal set; }

        /// <summary>
        /// Creates a new instance of the ServiceLocationAssemblyScanner class.
        /// </summary>
        public ServiceLocationAssemblyScanner()
        {
            this.GetRegistrarTypes = this.InternalGetRegistrarTypes;
            this.ServiceRegistrarFactory = new ServiceLocationRegistrarFactory();
        }

        /// <inheritdoc/>
        internal IEnumerable<Type> GetNewRegistrarTypes()
        {
            var newRegistrars = new List<Type>();
            if (this.HasNewAssemblies)
            {
                newRegistrars = this.GetRegistrarTypes().ToList();
            }

            return newRegistrars;
        }

        /// <summary>
        /// Gets an enumeration of service registrar objects from the given enumeration of types.
        /// </summary>
        /// <param name="registrarTypes">An enumeration of Type objects.</param>
        /// <returns>An enumeration of service location registrars.</returns>
        internal IEnumerable<IServiceLocationRegistrar> GetRegistrars(IEnumerable<Type> registrarTypes)
        {
           return (from t in registrarTypes
                           select ServiceRegistrarFactory.Create(t)).ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<IServiceLocationRegistrar> GetNewRegistrars()
        {
            var newRegistrars = this.GetNewRegistrarTypes().ToList();
            this._registrars.All(newRegistrars.Remove);
            this._registrars.AddRange(newRegistrars);
            this.HasNewAssemblies = false;

            return GetRegistrars(newRegistrars);
        }

        /// <inheritdoc/>
        public IEnumerable<IServiceLocationRegistrar> GetRegistrars()
        {
            var newRegistrars = this.GetNewRegistrarTypes().ToList();
            
            this._registrars.All(newRegistrars.Remove);
            this._registrars.AddRange(newRegistrars);
            this.HasNewAssemblies = false;

            return GetRegistrars(this._registrars);
        }

        /// <inheritdoc/>
        public void AddAssembly(Assembly target)
        {
            if (!this._assemblies.Contains(target))
            {
                this._assemblies.Add(target);
                this.HasNewAssemblies = true;
            }
        }

        /// <summary>
        /// Gets a list of types for any services registrars in the current application domain.
        /// </summary>
        /// <returns>A list of types.</returns>
        internal IEnumerable<Type> InternalGetRegistrarTypes()
        {
            var rawTypes = new List<Type>();
            var serviceRegistrarType = typeof (IServiceLocationRegistrar);

            foreach (var assembly in this._assemblies)
            {
                try
                {
                    rawTypes.AddRange(assembly.GetDefinedTypes());
                }
                catch (ReflectionTypeLoadException loadEx)
                {
                    var foundTypes = (from t in loadEx.Types where t != null select t).ToList();
                    rawTypes.AddRange(foundTypes);
                }
            }

            var rawRegistrarTypes = new List<Type>();
            foreach (var type in rawTypes)
            {
                if (type.IsInterface())
                {
                    continue;
                }

                if (serviceRegistrarType.IsAssignableFrom(type) && type.GetDefinedConstructors().Any(c => !c.GetParameters().Any()))
                {
                    rawRegistrarTypes.Add(type);
                }
            }

            return rawRegistrarTypes;
        }
    }
}
