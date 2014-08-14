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

namespace OpenStack.Compute
{
    /// <summary>
    /// Represents a key pair on the remote OpenStack instance.
    /// </summary>
    public class ComputeKeyPair
    {
        /// <summary>
        /// GEts the name of the key pair.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The public key for this key pair.
        /// </summary>
        public string PublicKey { get; internal set; }

        /// <summary>
        /// The fingerprint for this key pair.
        /// </summary>
        public string Fingerprint { get; internal set; }

        /// <summary>
        /// Creates a new instance of the ComputeKeyPair class.
        /// </summary>
        /// <param name="name">The name for the key pair.</param>
        /// <param name="publicKey">The public key for the key pair.</param>
        /// <param name="fingerprint">The fingerprint for the key pair.</param>
        public ComputeKeyPair(string name, string publicKey, string fingerprint)
        {
            this.Name = name;
            this.PublicKey = publicKey;
            this.Fingerprint = fingerprint;
        }
    }
}
