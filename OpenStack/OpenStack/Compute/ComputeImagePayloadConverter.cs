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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Common;
using System.Linq;

namespace OpenStack.Compute
{
    using System;

    /// <inheritdoc/>
    internal class ComputeImagePayloadConverter : IComputeImagePayloadConverter
    {
        /// <inheritdoc/>
        public ComputeImage ConvertImage(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "A null or empty compute image payload cannot be converted.");

            try
            {
                var token = JToken.Parse(payload);
                return ConvertImage(token["image"]);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute image payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        internal ComputeImage ConvertImage(JToken imageToken)
        {
            var name = string.Empty;
            var id = string.Empty;
            try
            {
                name = (string) imageToken["name"];
                id = (string) imageToken["id"];

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id))
                {
                    throw new FormatException();
                }

                var status = imageToken["status"] == null ? string.Empty : (string)imageToken["status"];
                var created = imageToken["created"] == null ? DateTime.MinValue : (DateTime)imageToken["created"];
                var updated = imageToken["updated"] == null ? DateTime.MinValue : (DateTime)imageToken["updated"];
                var minRam = imageToken["minRam"] == null ? 0 : (int)imageToken["minRam"];
                var minDisk = imageToken["minDisk"] == null ? 0 : (int)imageToken["minDisk"]; ;
                var progress = imageToken["progress"] == null ? 0 : (int)imageToken["progress"]; ;

                var permalink = string.Empty;
                var publicLink = string.Empty;
                var links = imageToken["links"];
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

                var metadata = new Dictionary<string, string>();
                var metadataToken = imageToken["metadata"];
                if (metadataToken != null)
                {
                    metadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(metadataToken.ToString());
                }

                return new ComputeImage(id, name, new Uri(publicLink), new Uri(permalink), metadata, status, created, updated, minDisk, minRam, progress);
            }
            catch (Exception ex)
            {
                var msg = "Compute image payload could not be parsed.";
                if (!string.IsNullOrEmpty(name) && imageToken != null)
                {
                    msg = string.Format(
                        "Compute image '{0}' with Id '{1}' payload could not be parsed. Payload: '{2}'", name, id,
                        imageToken);
                }
                else if (imageToken != null)
                {
                    msg = string.Format("Compute image payload could not be parsed. Payload: '{0}'", imageToken);
                }

                throw new FormatException(msg, ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ComputeImage> ConvertImages(string payload)
        {
            payload.AssertIsNotNull("payload", "A null compute images payload cannot be converted.");

            var images = new List<ComputeImage>();

            if (String.IsNullOrEmpty(payload))
            {
                return images;
            }

            try
            {
                var payloadToken = JToken.Parse(payload);
                var flavorsArray = payloadToken["images"];
                images.AddRange(flavorsArray.Select(ConvertImage));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute images payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return images;
        }
    }
}
