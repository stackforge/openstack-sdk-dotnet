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
using Newtonsoft.Json.Linq;
using OpenStack.Common;

namespace OpenStack.Compute
{
    /// <inheritdoc/>
    internal class ComputeServerPayloadConverter : IComputeServerPayloadConverter
    {
        /// <inheritdoc/>
        public ComputeServer ConvertSummary(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "A null or empty compute server payload cannot be converted.");

            try
            {
                var token = JToken.Parse(payload);
                return ConvertServerSummary(token["server"]);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute server payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <inheritdoc/>
        public ComputeServer Convert(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "A null or empty compute server payload cannot be converted.");

            try
            {
                var token = JToken.Parse(payload);
                return ConvertServer(token["server"]);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute server payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a payload with a list of servers to an enumerable collection of ComputeServer objects. 
        /// </summary>
        /// <param name="payload">The payload to convert.</param>
        /// <returns>An enumerable list of ComputeServers.</returns>
        public IEnumerable<ComputeServer> ConvertServers(string payload)
        {
            payload.AssertIsNotNull("payload", "A null compute servers payload cannot be converted.");

            var servers = new List<ComputeServer>();

            if (String.IsNullOrEmpty(payload))
            {
                return servers;
            }

            try
            {
                var payloadToken = JToken.Parse(payload);
                var serverArray = payloadToken["servers"];
                servers.AddRange(serverArray.Select(ConvertServerItem));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute servers payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return servers;
        }

        /// <summary>
        /// Converts a json token into a ComputeServer object.
        /// </summary>
        /// <param name="serverToken">The json Token to convert.</param>
        /// <returns>A ComputeServer object.</returns>
        internal ComputeServer ConvertServerSummary(JToken serverToken)
        {
            
            var id = string.Empty;
            var adminPassword = string.Empty;
            
            try
            {
                adminPassword = (string)serverToken["adminPass"];
                id = (string)serverToken["id"];

                if (string.IsNullOrEmpty(adminPassword) || string.IsNullOrEmpty(id))
                {
                    throw new FormatException();
                }

                var permalink = string.Empty;
                var publicLink = string.Empty;
                var links = serverToken["links"];
                if (links != null)
                {
                    foreach (var linkToken in links)
                    {
                        switch (linkToken["rel"].Value<string>().ToLower())
                        {
                            case "self":
                                publicLink = linkToken["href"].Value<string>();
                                break;
                            case "bookmark":
                                permalink = linkToken["href"].Value<string>();
                                break;
                        }
                    }
                }

                return new ComputeServer(id, id, adminPassword, new Uri(publicLink), new Uri(permalink), new Dictionary<string, string>());
            }
            catch (Exception ex)
            {
                var msg = "Compute server payload could not be parsed.";
                if (!string.IsNullOrEmpty(id) && serverToken != null)
                {
                    msg = string.Format(
                        "Compute server with Id '{0}' payload could not be parsed. Payload: '{1}'", id, serverToken);
                }
                else if (serverToken != null)
                {
                    msg = string.Format("Compute server payload could not be parsed. Payload: '{0}'", serverToken);
                }

                throw new FormatException(msg, ex);
            }
        }

        /// <summary>
        /// Converts a json token into a ComputeServer object.
        /// </summary>
        /// <param name="serverToken">The json Token to convert.</param>
        /// <returns>A ComputeServer object.</returns>
        internal ComputeServer ConvertServerItem(JToken serverToken)
        {

            var id = string.Empty;
            var name = string.Empty;

            try
            {
                name = (string)serverToken["name"];
                id = (string)serverToken["id"];

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id))
                {
                    throw new FormatException();
                }

                var permalink = string.Empty;
                var publicLink = string.Empty;
                var links = serverToken["links"];
                if (links != null)
                {
                    foreach (var linkToken in links)
                    {
                        switch (linkToken["rel"].Value<string>().ToLower())
                        {
                            case "self":
                                publicLink = linkToken["href"].Value<string>();
                                break;
                            case "bookmark":
                                permalink = linkToken["href"].Value<string>();
                                break;
                        }
                    }
                }

                return new ComputeServer(id, name, new Uri(publicLink), new Uri(permalink), new Dictionary<string, string>());
            }
            catch (Exception ex)
            {
                var msg = "Compute server payload could not be parsed.";
                if (!string.IsNullOrEmpty(id) && serverToken != null)
                {
                    msg = string.Format(
                        "Compute server with Id '{0}' payload could not be parsed. Payload: '{1}'", id, serverToken);
                }
                else if (serverToken != null)
                {
                    msg = string.Format("Compute server payload could not be parsed. Payload: '{0}'", serverToken);
                }

                throw new FormatException(msg, ex);
            }
        }

        /// <summary>
        /// Converts a json token into a ComputeServer object.
        /// </summary>
        /// <param name="serverToken">The json Token to convert.</param>
        /// <returns>A ComputeServer object.</returns>
        internal ComputeServer ConvertServer(JToken serverToken)
        {

            var id = string.Empty;
            var name = string.Empty;
            var status = string.Empty;

            try
            {
                name = (string)serverToken["name"];
                status = (string)serverToken["status"];
                id = (string)serverToken["id"];

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(status))
                {
                    throw new FormatException();
                }

                var permalink = string.Empty;
                var publicLink = string.Empty;
                var links = serverToken["links"];
                if (links != null)
                {
                    foreach (var linkToken in links)
                    {
                        switch (linkToken["rel"].Value<string>().ToLower())
                        {
                            case "self":
                                publicLink = linkToken["href"].Value<string>();
                                break;
                            case "bookmark":
                                permalink = linkToken["href"].Value<string>();
                                break;
                        }
                    }
                }

                var progress = serverToken["progress"] != null ? (int)serverToken["progress"] : 0;

                return new ComputeServer(id, name, status.GetComputeServerStatus(), progress,  new Uri(publicLink), new Uri(permalink), new Dictionary<string, string>());
            }
            catch (Exception ex)
            {
                var msg = "Compute server payload could not be parsed.";
                if (!string.IsNullOrEmpty(id) && serverToken != null)
                {
                    msg = string.Format(
                        "Compute server with Id '{0}' payload could not be parsed. Payload: '{1}'", id, serverToken);
                }
                else if (serverToken != null)
                {
                    msg = string.Format("Compute server payload could not be parsed. Payload: '{0}'", serverToken);
                }

                throw new FormatException(msg, ex);
            }
        }
    }
}
