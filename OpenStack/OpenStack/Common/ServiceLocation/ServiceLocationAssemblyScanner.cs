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
            var rawTypeInfos = new List<TypeInfo>();
            var serviceRegistrarTypeInfo = typeof (IServiceLocationRegistrar).GetTypeInfo();

            foreach (var assembly in this._assemblies)
            {
                try
                {
                    rawTypeInfos.AddRange(assembly.DefinedTypes);
                }
                catch (ReflectionTypeLoadException loadEx)
                {
                    var foundTypes = (from t in loadEx.Types where t != null select t.GetTypeInfo()).ToList();
                    rawTypeInfos.AddRange(foundTypes);
                }
            }

            var rawRegistrarTypes = new List<Type>();
            foreach (var typeInfo in rawTypeInfos)
            {
                if (typeInfo.IsInterface)
                {
                    continue;
                }

                if (serviceRegistrarTypeInfo.IsAssignableFrom(typeInfo) && typeInfo.DeclaredConstructors.Any(c => !c.GetParameters().Any()))
                {
                    //This is an odd 'feature' in the PCL reflection code. Basically using TypeInfo.GetType() does not give you the 'real' type
                    //so when the caller tries to use the returned type in a call to Activator.CreateInstance, it will try and create a RuntimeType object (which does not have a default constructor)
                    //and fail. But by asking the assembly that the type came from for a reference to the type, the type that is returned can be created. Weird I know... but hey.
                    var rawType = typeInfo.Assembly.GetType(typeInfo.FullName); 

                    rawRegistrarTypes.Add(rawType);
                }
            }

            return rawRegistrarTypes;
        }
    }
}
