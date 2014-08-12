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

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using System.Linq;

namespace OpenStack.Network
{
    using System;

    /// <inheritdoc/>
    internal class FloatingIpPayloadConverter : IFloatingIpPayloadConverter
    {
        /// <inheritdoc/>
        public FloatingIp Convert(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "A null or empty floating ip payload cannot be converted.");

            try
            {
                var token = JToken.Parse(payload);
                return ConvertFloatingIp(token["floatingip"]);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Floating IP payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a json token into a FloatingIp object.
        /// </summary>
        /// <param name="floatingIpToken">The json Token to convert.</param>
        /// <returns>A FloatingIp object.</returns>
        internal FloatingIp ConvertFloatingIp(JToken floatingIpToken)
        {
            var floatingIpAddress = string.Empty;
            var id = string.Empty;
            var status = string.Empty;

            try
            {
                floatingIpAddress = (string)floatingIpToken["floating_ip_address"];
                id = (string)floatingIpToken["id"];
                status = (string) floatingIpToken["status"];

                if (string.IsNullOrEmpty(floatingIpAddress) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(status))
                {
                    throw new FormatException();
                }

                return new FloatingIp(id, floatingIpAddress, status.GetFloatingIpStatus());
            }
            catch (Exception ex)
            {
                var msg = "Floating IP payload could not be parsed.";
                if (!string.IsNullOrEmpty(id) && floatingIpToken != null)
                {
                    msg = string.Format(
                        "Floating IP with Id '{0}' payload could not be parsed. Payload: '{1}'", id, floatingIpToken);
                }
                else if (floatingIpToken != null)
                {
                    msg = string.Format("Floating IP payload could not be parsed. Payload: '{0}'", floatingIpToken);
                }

                throw new FormatException(msg, ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<FloatingIp> ConvertFloatingIps(string payload)
        {
            payload.AssertIsNotNull("payload", "A null floating IPs payload cannot be converted.");

            var floatingIps = new List<FloatingIp>();

            if (String.IsNullOrEmpty(payload))
            {
                return floatingIps;
            }

            try
            {
                var payloadToken = JToken.Parse(payload);
                var floatingIpArray = payloadToken["floatingips"];
                floatingIps.AddRange(floatingIpArray.Select(ConvertFloatingIp));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Floating IPs payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return floatingIps;
        }
    }
}
