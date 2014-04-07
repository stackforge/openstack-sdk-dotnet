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

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using OpenStack.Common.Http;

namespace OpenStack.Test
{
    public static class TestHelper
    {
        public static MemoryStream CreateStream(string input)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            return new MemoryStream(byteArray);
        }

        public static string GetStringFromStream(Stream input)
        {
            var reader = new StreamReader(input);
            return reader.ReadToEnd();
        }

        public static IHttpResponseAbstraction CreateErrorResponse()
        {
            return new HttpResponseAbstraction(null, new HttpHeadersAbstraction(), HttpStatusCode.InternalServerError);
        }

        public static IHttpResponseAbstraction CreateResponse(HttpStatusCode code)
        {
            return new HttpResponseAbstraction(null, new HttpHeadersAbstraction(), code);
        }

        public static IHttpResponseAbstraction CreateResponse(HttpStatusCode code, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var abstractionHeaders = new HttpHeadersAbstraction();
            foreach (var header in headers)
            {
                abstractionHeaders.Add(header.Key, header.Value);
            }
            return new HttpResponseAbstraction(null, abstractionHeaders, code);
        }

        public static IHttpResponseAbstraction CreateResponse(HttpStatusCode code, IEnumerable<KeyValuePair<string, string>> headers, Stream content)
        {
            var abstractionHeaders = new HttpHeadersAbstraction();
            foreach (var header in headers)
            {
                abstractionHeaders.Add(header.Key, header.Value);
            }
            return new HttpResponseAbstraction(content, abstractionHeaders, code);
        }
    }
}
