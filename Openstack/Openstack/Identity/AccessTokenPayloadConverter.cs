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

namespace Openstack.Identity
{
    using System;
    using System.Web;
    using Newtonsoft.Json.Linq;
    using Openstack.Common;

    /// <inheritdoc/>
    internal class AccessTokenPayloadConverter : IAccessTokenPayloadConverter
    {
        /// <inheritdoc/>
        public string Convert(string payload)
        {
            payload.AssertIsNotNull("payload", "A null Storage Container payload cannot be converted.");

            if (String.IsNullOrEmpty(payload))
            {
                return string.Empty;
            }

            try
            {
                var obj = JObject.Parse(payload);
                var token = (string)obj["access"]["token"]["id"];

                if (token == null)
                {
                    throw new HttpParseException(string.Format("Access token payload could not be parsed. Token is null. Payload: '{0}'", payload));
                }

                return token;
            }
            catch (Exception ex)
            {
                throw new HttpParseException(string.Format("Access token payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }
    }
}
