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

using System.IO;
using System.Threading.Tasks;

namespace OpenStack.Common
{
    /// <summary>
    /// Static class for extending the Stream class.
    /// </summary>
    public static class StreamExtentions
    {
        /// <summary>
        /// Copies the given stream into another stream asynchronously.
        /// </summary>
        /// <param name="input">The given stream.</param>
        /// <param name="output">The stream that will be copied into.</param>
        /// <returns>An asynchronous task.</returns>
        public static Task CopyAsync(this Stream input, Stream output)
        {
            return Task.Factory.StartNew(() => input.CopyTo(output));
        }
    }
}
