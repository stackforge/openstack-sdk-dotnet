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
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OpenStack.Common.ServiceLocation
{
    /// <inheritdoc/>
    public class ServiceLocator : IServiceLocator
    {
        private readonly ServiceLocationManager _runtimeManager;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> _overrideServices = new Dictionary<Type, object>();
        private readonly IServiceLocationAssemblyScanner _scanner = new ServiceLocationAssemblyScanner();

        internal ServiceLocator()
        {
            this._runtimeManager = new ServiceLocationRuntimeManager(this);
            this._services.Add(typeof(IServiceLocationRuntimeManager), this._runtimeManager);
            this._services.Add(typeof(IServiceLocationOverrideManager), new ServiceLocationOverrideManager(this));
            this._scanner.AddAssembly(this.GetType().GetAssembly());
            this.RegisterServices();
        }

        internal void RegisterServices(IEnumerable<IServiceLocationRegistrar> registrars)
        {
           foreach (var serviceLocationRegistrar in registrars)
            {
                serviceLocationRegistrar.Register(this._runtimeManager, this);
            }
        }

        internal void RegisterServices()
        {
            var registrars = this._scanner.GetRegistrars().ToList();
            this.RegisterServices(registrars);
        }

        /// <inheritdoc/>
        public T Locate<T>()
        {
            return (T)this.Locate(typeof(T));
        }

        public void EnsureAssemblyRegistration(Assembly target)
        {
            this._scanner.AddAssembly(target);
            var registrars = this._scanner.GetNewRegistrars();
            this.RegisterServices(registrars);
        }

        /// <summary>
        /// Locates an implementation of the given Type.
        /// </summary>
        /// <param name="type">The Type to locate.</param>
        /// <returns>The implementation of the given type that has been located.</returns>
        internal object Locate(Type type)
        {
            var retval = this.InternalLocate(type);
            if (retval != null)
            {
                return retval;
            }

            if (this._scanner.HasNewAssemblies)
            {
                this.RegisterServices();
                retval = this.InternalLocate(type);
            }

            if (retval != null)
            {
                return retval;
            }

            var message = string.Format(CultureInfo.InvariantCulture,
                "Service '{0}' has not been registered",
                type.FullName);
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Locates an implementation of the given Type.
        /// </summary>
        /// <param name="type">The Type to locate.</param>
        /// <returns>The implementation of the given type that has been located.</returns>
        private object InternalLocate(Type type)
        {
            if (ReferenceEquals(type,
                null))
            {
                throw new ArgumentNullException("type");
            }
            object runtimeVersion = null;
            object overrideVersion = null;

            // First try to get a an override
            if (!this._overrideServices.TryGetValue(type, out overrideVersion))
            {
                //if no override, then try to get the actual service.
                this._services.TryGetValue(type, out runtimeVersion);
            }

            return overrideVersion ?? runtimeVersion;
        }

        /// <inheritdoc/>
        private class ServiceLocationRuntimeManager : ServiceLocationManager, IServiceLocationRuntimeManager
        {
            private readonly ServiceLocator _locator;

            /// <inheritdoc/>
            public ServiceLocationRuntimeManager(ServiceLocator locator)
            {
                this._locator = locator;
            }

            /// <inheritdoc />
            public override void RegisterServiceInstance(Type type, object instance)
            {
                ThrowIfNullInstance(type,
                                    instance);
                ThrowIfInvalidRegistration(type,
                                           instance.GetType());
                var internalManager = new RuntimeRegistrationManager(this._locator);
                internalManager.RegisterServiceInstance(type,
                                                 instance);
                foreach (KeyValuePair<Type, object> keyValuePair in internalManager.GetDiscovered())
                {
                    this._locator._services[keyValuePair.Key] = keyValuePair.Value;
                }
            }
        }

        /// <inheritdoc/>
        private class ServiceLocationOverrideManager : ServiceLocationManager, IServiceLocationOverrideManager
        {
            private readonly ServiceLocator _locator;

            /// <inheritdoc/>
            public ServiceLocationOverrideManager(ServiceLocator locator)
            {
                this._locator = locator;
            }

            /// <inheritdoc/>
            public override void RegisterServiceInstance(Type type, object instance)
            {
                ThrowIfNullInstance(type,
                                    instance);
                ThrowIfInvalidRegistration(type,
                                           instance.GetType());

                this._locator._overrideServices[type] = instance;
            }
        }
    }
}
