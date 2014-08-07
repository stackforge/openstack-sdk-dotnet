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
    internal class NetworkPayloadConverter : INetworkPayloadConverter
    {
        /// <inheritdoc/>
        public Network Convert(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "A null or empty network payload cannot be converted.");

            try
            {
                var token = JToken.Parse(payload);
                return ConvertNetwork(token["network"]);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Network payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a json token into a Network object.
        /// </summary>
        /// <param name="networkToken">The json Token to convert.</param>
        /// <returns>A Network object.</returns>
        internal Network ConvertNetwork(JToken networkToken)
        {
            var name = string.Empty;
            var id = string.Empty;
            var status = string.Empty;

            try
            {
                name = (string)networkToken["name"];
                id = (string)networkToken["id"];
                status = (string) networkToken["status"];

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(status))
                {
                    throw new FormatException();
                }

                return new Network(id, name, status.GetNetworkStatus());
            }
            catch (Exception ex)
            {
                var msg = "Network payload could not be parsed.";
                if (!string.IsNullOrEmpty(name) && networkToken != null)
                {
                    msg = string.Format(
                        "Network '{0}' with Id '{1}' payload could not be parsed. Payload: '{2}'", name, id,
                        networkToken);
                }
                else if (networkToken != null)
                {
                    msg = string.Format("Network payload could not be parsed. Payload: '{0}'", networkToken);
                }

                throw new FormatException(msg, ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Network> ConvertNetworks(string payload)
        {
            payload.AssertIsNotNull("payload", "A null networks payload cannot be converted.");

            var networks = new List<Network>();

            if (String.IsNullOrEmpty(payload))
            {
                return networks;
            }

            try
            {
                var payloadToken = JToken.Parse(payload);
                var networksArray = payloadToken["networks"];
                networks.AddRange(networksArray.Select(ConvertNetwork));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Networks payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return networks;
        }
    }
}
