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

using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OpenStack.Common.Http
{
    public class HttpResponseAbstraction : IHttpResponseAbstraction
    {
        public HttpResponseAbstraction(Stream content, IHttpHeadersAbstraction headers, HttpStatusCode status)
        {
            this.Headers = headers ?? new HttpHeadersAbstraction();
            this.StatusCode = status;
            this.Content = content;
        }

        public Stream Content { get; private set; }

        public IHttpHeadersAbstraction Headers { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public async Task<string> ReadContentAsStringAsync()
        {
            using (var sr = new StreamReader(this.Content))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
}
