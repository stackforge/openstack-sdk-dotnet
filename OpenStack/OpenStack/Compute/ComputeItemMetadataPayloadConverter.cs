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
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Common;

namespace OpenStack.Compute
{
    internal class ComputeItemMetadataPayloadConverter : IComputeItemMetadataPayloadConverter
    {
        /// <inheritdoc/>
        public IDictionary<string, string> Convert(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "A null or empty compute item metadata payload cannot be converted.");
            var metdata = new Dictionary<string, string>();

            try
            {
                var token = JToken.Parse(payload);
                var metadataToken = token["metadata"];
                if (metadataToken == null)
                {
                   throw new FormatException();
                }
                metdata = JsonConvert.DeserializeObject<Dictionary<string, string>>(metadataToken.ToString());
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute item metadata payload could not be parsed. Payload: '{0}'", payload), ex);
            }
            return metdata;
        }

        /// <inheritdoc/>
        public string Convert(IDictionary<string, string> metadata)
        {
            metadata.AssertIsNotNull("metadata", "Cannot convert compute item metadata with null metadata.");
            
            var payload = new StringBuilder();
            payload.Append("{ \"metadata\" : {");
            var isFirst = true;

            foreach (var item in metadata)
            {
                if (!isFirst)
                {
                    payload.Append(",");
                }

                payload.AppendFormat("\"{0}\":\"{1}\"",item.Key, item.Value);
                isFirst = false;
            }

            payload.Append("}}");
            return payload.ToString();
        }
    }
}
