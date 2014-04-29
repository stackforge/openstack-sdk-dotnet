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
using System.Linq;
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Identity
{
    /// <inheritdoc/>
    internal class OpenStackServiceCatalogPayloadConverter : IOpenStackServiceCatalogPayloadConverter
    {
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the OpenStackServiceCatalogPayloadConverter class.
        /// </summary>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        public OpenStackServiceCatalogPayloadConverter(IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a service catalog payload converter with a null service locator.");
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public OpenStackServiceCatalog Convert(string payload)
        {
            payload.AssertIsNotNull("payload", "A null service catalog payload cannot be converted.");

            var catalog = new OpenStackServiceCatalog();

            if (String.IsNullOrEmpty(payload))
            {
                return catalog;
            }

            try
            {
                var obj = JObject.Parse(payload);
                var defArray = obj["access"]["serviceCatalog"];
                catalog.AddRange(defArray.Select(ConvertServiceDefinition));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Service catalog payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return catalog;
        }

        /// <summary>
        /// Converts a Json token that represents a service definition into a POCO object.
        /// </summary>
        /// <param name="serviceDef">The token.</param>
        /// <returns>The service definition.</returns>
        internal OpenStackServiceDefinition ConvertServiceDefinition(JToken serviceDef)
        {
            var converter = this.ServiceLocator.Locate<IOpenStackServiceDefinitionPayloadConverter>();
            return converter.Convert(serviceDef.ToString());
        }
    }
}
