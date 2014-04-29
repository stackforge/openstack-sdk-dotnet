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
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using OpenStack.Common.ServiceLocation;

namespace OpenStack.Identity
{
    /// <inheritdoc/>
    internal class OpenStackServiceDefinitionPayloadConverter : IOpenStackServiceDefinitionPayloadConverter
    {
        internal IServiceLocator ServiceLocator;

        /// <summary>
        /// Creates a new instance of the OpenStackServiceDefinitionPayloadConverter class.
        /// </summary>
        /// <param name="serviceLocator">A service locator to be used to locate/inject dependent services.</param>
        public OpenStackServiceDefinitionPayloadConverter(IServiceLocator serviceLocator)
        {
            serviceLocator.AssertIsNotNull("serviceLocator", "Cannot create a service definition payload converter with a null service locator.");
            this.ServiceLocator = serviceLocator;
        }

        /// <inheritdoc/>
        public OpenStackServiceDefinition Convert(string payload)
        {
            payload.AssertIsNotNull("payload", "A null service catalog payload cannot be converted.");

            try
            {
                var serviceDefinition = JObject.Parse(payload);
                var name = (string)serviceDefinition["name"];
                var type = (string)serviceDefinition["type"];

                var endpoints = new List<OpenStackServiceEndpoint>();
                endpoints.AddRange(serviceDefinition["endpoints"].Select(ConvertEndpoint));

                return new OpenStackServiceDefinition(name, type, endpoints);
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Service definition payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a Json token that represents a service endpoint into a POCO object.
        /// </summary>
        /// <param name="endpoint">The token.</param>
        /// <returns>A service endpoint.</returns>
        internal OpenStackServiceEndpoint ConvertEndpoint(JToken endpoint)
        {
            var converter = this.ServiceLocator.Locate<IOpenStackServiceEndpointPayloadConverter>();
            return converter.Convert(endpoint.ToString());
        }
    }
}
