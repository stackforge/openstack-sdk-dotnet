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
using Newtonsoft.Json.Serialization;
using OpenStack.Common;
using System.Linq;

namespace OpenStack.Compute
{
    using System;

    /// <inheritdoc/>
    internal class ComputeFlavorPayloadConverter : IComputeFlavorPayloadConverter
    {
        /// <inheritdoc/>
        public ComputeFlavor ConvertFlavor(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "A null or empty compute flavor payload cannot be converted.");

            try
            {
                var token = JToken.Parse(payload);
                return ConvertFlavor(token["flavor"]);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute flavor payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        internal ComputeFlavor ConvertFlavor(JToken flavorToken)
        {
            var name = string.Empty;
            var id = string.Empty;
            try
            {
                name = (string) flavorToken["name"];
                id = (string) flavorToken["id"];

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id))
                {
                    throw new FormatException();
                }

                var ram = (string) flavorToken["ram"];
                var vcpus = (string) flavorToken["vcpus"];
                var disk = (string) flavorToken["disk"];

                var permalink = string.Empty;
                var publicLink = string.Empty;
                var links = flavorToken["links"];
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

                return new ComputeFlavor(id, name, ram, vcpus, disk, new Uri(publicLink), new Uri(permalink), new Dictionary<string, string>());
            }
            catch (Exception ex)
            {
                var msg = "Compute flavor payload could not be parsed.";
                if (!string.IsNullOrEmpty(name) && flavorToken != null)
                {
                    msg = string.Format(
                        "Compute flavor '{0}' with Id '{1}' payload could not be parsed. Payload: '{2}'", name, id,
                        flavorToken);
                }
                else if (flavorToken != null)
                {
                    msg = string.Format("Compute flavor payload could not be parsed. Payload: '{0}'", flavorToken);
                }

                throw new FormatException(msg, ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ComputeFlavor> ConvertFlavors(string payload)
        {
            payload.AssertIsNotNull("payload", "A null compute flavor payload cannot be converted.");

            var flavors = new List<ComputeFlavor>();

            if (String.IsNullOrEmpty(payload))
            {
                return flavors;
            }

            try
            {
                var payloadToken = JToken.Parse(payload);
                var flavorsArray = payloadToken["flavors"];
                flavors.AddRange(flavorsArray.Select(ConvertFlavor));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute flavors payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return flavors;
        }
    }
}
