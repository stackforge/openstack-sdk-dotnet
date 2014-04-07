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
        private IServiceLocationRegistrarFactory ServiceRegistrarFactory { get; set; }
        private readonly List<Type> _registrars = new List<Type>();
        private readonly List<Assembly> _assemblies = new List<Assembly>(); 

        /// <summary>
        /// Gets and sets a list of assemblies that have been scanned.
        /// </summary>
        internal Func<IEnumerable<Assembly>> GetAllAssemblies { get; set; }

        /// <summary>
        /// Creates a new instance of the ServiceLocationAssemblyScanner class.
        /// </summary>
        public ServiceLocationAssemblyScanner()
        {
            this.GetAllAssemblies = this.InternalGetAllAssemblies;
            this.ServiceRegistrarFactory = new ServiceLocationRegistrarFactory();
        }

        /// <summary>
        /// Gets a list of all assemblies in the current application domain.
        /// </summary>
        /// <returns>A list of assembly objects.</returns>
        internal IEnumerable<Assembly> InternalGetAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <inheritdoc/>
        public bool HasNewAssemblies()
        {
            var assemblies = this.GetAllAssemblies().ToList();
            this._assemblies.All(assemblies.Remove);
            this._assemblies.AddRange(assemblies);
            return assemblies.Any();
        }

        /// <inheritdoc/>
        public IEnumerable<IServiceLocationRegistrar> GetRegistrars()
        {
            var newRegistrars = new List<Type>();
            if (this.HasNewAssemblies())
            {
                newRegistrars = this.GetRegistrarTypes().ToList();
            }

            this._registrars.All(newRegistrars.Remove);
            this._registrars.AddRange(newRegistrars);

            var objects = (from t in this._registrars
                           select ServiceRegistrarFactory.Create(t)).ToList();
            return objects;
        }

        /// <summary>
        /// Gets a list of types for any services registrars in the current application domain.
        /// </summary>
        /// <returns>A list of types.</returns>
        internal IEnumerable<Type> GetRegistrarTypes()
        {
            var comparer = new AssemblyNameEqualityComparer();
            var rawTypes = new List<Type>();

            var scansedAssemblies = this.GetAllAssemblies();
            var workingAssemblies = (from s in scansedAssemblies
                                     where s.GetReferencedAssemblies().Contains(typeof(IServiceLocationRegistrar).Assembly.GetName(), comparer)
                                     select s).ToList();
            workingAssemblies.Add(this.GetType().Assembly);

            foreach (var assembly in workingAssemblies)
            {
                try
                {
                    rawTypes.AddRange(assembly.GetTypes());
                }
                catch (ReflectionTypeLoadException loadEx)
                {
                    var foundTypes = (from t in loadEx.Types where t != null select t).ToList();
                    rawTypes.AddRange(foundTypes);
                }
            }

           var unorderedTypes = new Queue<Type>();
            foreach (var type in rawTypes)
            {
                if (type.IsInterface)
                {
                    continue;
                }

                if (typeof(IServiceLocationRegistrar).IsAssignableFrom(type) && !ReferenceEquals(type.GetConstructor(new Type[0]), null))
                {
                    unorderedTypes.Enqueue(type);
                }
            }

            var registrarTypes = new List<Type>();
            while (unorderedTypes.Count > 0)
            {
                var type = unorderedTypes.Dequeue();
                var addToTypesList = true;
                foreach (var stackType in unorderedTypes)
                {
                    if (!type.Assembly.GetReferencedAssemblies().Contains(stackType.Assembly.GetName(), comparer))
                    {
                        continue;
                    }
             
                    addToTypesList = false;
                    unorderedTypes.Enqueue(type);
                    break;
                }
                if (addToTypesList)
                {
                    registrarTypes.Add(type);
                }
            }

            return registrarTypes;
        }
    }
}
