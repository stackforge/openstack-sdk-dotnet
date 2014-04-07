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

namespace OpenStack.Common.ServiceLocation
{
    /// <summary>
    /// Represents an object that can manage service location registration.
    /// </summary>
    public interface IServiceLocationManager
    {
        /// <summary>
        /// Registers an instance of a service with the service locator.
        /// </summary>
        /// <typeparam name="TService">The interface of the service to be registered.</typeparam>
        /// <param name="instance">An instance of the service to be returned by the service locator.</param>
        void RegisterServiceInstance<TService>(TService instance);

        /// <summary>
        /// Registers an instance of a service with the service locator.
        /// </summary>
        /// <param name="type">The interface of the service to be registered.</param>
        /// <param name="instance">An instance of the service to be returned by the service locator.</param>
        void RegisterServiceInstance(Type type, object instance);

        /// <summary>
        /// Registers a type of a service with the service locator.
        /// </summary>
        /// <typeparam name="T">The interface of the service to be registered.</typeparam>
        /// <param name="type">A concrete type of the service to be returned by the service locator.</param>
        void RegisterServiceType<T>(Type type);

        /// <summary>
        /// Registers a type of a service with the service locator.
        /// </summary>
        /// <typeparam name="TInterface">The interface of the service to be registered.</typeparam>
        /// <typeparam name="TConcretion">A concrete type of the service to be returned by the service locator.</typeparam>
        void RegisterServiceType<TInterface, TConcretion>() where TConcretion : class, TInterface;

        /// <summary>
        /// Registers a type of a service with the service locator.
        /// </summary>
        /// <param name="type">The interface of the service to be registered.</param>
        /// <param name="registrationValue">A concrete type of the service to be returned by the service locator.</param>
        void RegisterServiceType(Type type, Type registrationValue);
    }
}
