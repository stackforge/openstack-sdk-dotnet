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

namespace OpenStack.Compute
{
    using System;

    /// <inheritdoc/>
    internal class ComputeKeyPairPayloadConverter : IComputeKeyPairPayloadConverter
    {
        /// <inheritdoc/>
        public ComputeKeyPair Convert(string payload)
        {
            payload.AssertIsNotNullOrEmpty("payload", "A null or empty compute key pair payload cannot be converted.");

            try
            {
                var token = JToken.Parse(payload);
                return Convert(token["keypair"]);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute key pair payload could not be parsed. Payload: '{0}'", payload), ex);
            }
        }

        /// <summary>
        /// Converts a Json token to a ComputeKeyPair object.
        /// </summary>
        /// <param name="keyPairToken">The token to convert.</param>
        /// <returns>A ComputeKeyPair object.</returns>
        internal ComputeKeyPair Convert(JToken keyPairToken)
        {
            var name = string.Empty;
            var fingerprint = string.Empty;
            var publicKey = string.Empty;
            try
            {
                name = (string) keyPairToken["name"];
                fingerprint = (string)keyPairToken["fingerprint"] ?? string.Empty;
                publicKey = (string)keyPairToken["public_key"] ?? string.Empty;

                if (string.IsNullOrEmpty(name))
                {
                    throw new FormatException();
                }

                return new ComputeKeyPair(name, publicKey, fingerprint);
            }
            catch (Exception ex)
            {
                var msg = "Compute key pair payload could not be parsed.";
                if (!string.IsNullOrEmpty(name) && keyPairToken != null)
                {
                    msg = string.Format(
                        "Compute key pair '{0}' payload could not be parsed. Payload: '{1}'", name, keyPairToken);
                }
                else if (keyPairToken != null)
                {
                    msg = string.Format("Compute key pair payload could not be parsed. Payload: '{0}'", keyPairToken);
                }

                throw new FormatException(msg, ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ComputeKeyPair> ConvertKeyPairs(string payload)
        {
            payload.AssertIsNotNull("payload", "A null compute key pair payload cannot be converted.");

            var keyPairs = new List<ComputeKeyPair>();

            if (String.IsNullOrEmpty(payload))
            {
                return keyPairs;
            }

            try
            {
                var payloadToken = JToken.Parse(payload);
                var keyPairArray = payloadToken["keypairs"];
                keyPairs.AddRange(keyPairArray.Select(token => Convert(token["keypair"])));
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    string.Format("Compute key pair payload could not be parsed. Payload: '{0}'", payload), ex);
            }

            return keyPairs;
        }
    }
}
