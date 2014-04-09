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

using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace OpenStack.Common.Http
{
    public class HttpHeadersAbstraction : IHttpHeadersAbstraction
    {
        private readonly Dictionary<string, IEnumerable<string>> _headers = new Dictionary<string, IEnumerable<string>>();

        public HttpHeadersAbstraction()
        {
        }

        public HttpHeadersAbstraction(HttpResponseHeaders headers)
        {
            foreach (var header in headers)
            {
                this._headers.Add(header.Key,header.Value);
            }
        }

        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
        {
            return this._headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._headers.GetEnumerator();
        }

        public void Add(string name, IEnumerable<string> values)
        {
            this._headers.Add(name,values);
        }

        public void AddRange(HttpResponseHeaders headers)
        {
            foreach (var header in headers)
            {
                this._headers.Add(header.Key, header.Value);
            }
        }

        public void AddRange(HttpContentHeaders headers)
        {
            foreach (var header in headers)
            {
                this._headers.Add(header.Key, header.Value);
            }
        }

        public void Add(string name, string value)
        {
            this._headers.Add(name, new List<string>() { value });
        }

        public void Clear()
        {
            this._headers.Clear();
        }

        public bool Contains(string name)
        {
            return this._headers.ContainsKey(name);
        }

        public IEnumerable<string> GetValues(string name)
        {
            return this._headers[name];
        }

        public void Remove(string name)
        {
            this._headers.Remove(name);
        }

        public bool TryGetValue(string name, out IEnumerable<string> values)
        {
            return this._headers.TryGetValue(name, out values);
        }

        public IEnumerable<string> this[string name]
        {
            get { return this._headers[name]; }
            set { this._headers[name] = value; }
        }
    }
}
