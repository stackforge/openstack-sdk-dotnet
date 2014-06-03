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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Common.Http;

namespace OpenStack.Test
{
    public abstract class RestSimulator : DisposableClass, IHttpAbstractionClient
    {
        protected RestSimulator()
        {
            this.Headers = new Dictionary<string, string>();
            this.Delay = TimeSpan.FromMilliseconds(0);
        }

        protected RestSimulator(CancellationToken token) : this()
        {
        }

        public HttpMethod Method { get; set; }

        public Uri Uri { get; set; }

        public Stream Content { get; set; }

        public IDictionary<string, string> Headers { get; private set; }

        public string ContentType { get; set; }

        public TimeSpan Timeout { get; set; }

        public TimeSpan Delay { get; set; }

        public Task<IHttpResponseAbstraction> SendAsync()
        {
            if (!this.Headers.ContainsKey("X-Auth-Token") || this.Headers["X-Auth-Token"] != "12345")
            {
                return Task.Factory.StartNew(() => TestHelper.CreateResponse(HttpStatusCode.Unauthorized));
            }
            IHttpResponseAbstraction retVal;
            switch (this.Method.ToString().ToLowerInvariant())
            {
                case "get":
                    retVal = HandleGet();
                    break;
                case "post":
                    retVal = HandlePost();
                    break;
                case "put":
                    retVal = HandlePut();
                    break;
                case "delete":
                    retVal = HandleDelete();
                    break;
                case "head":
                    retVal = HandleHead();
                    break;
                case "copy":
                    retVal = HandleCopy();
                    break;
                default:
                    retVal = TestHelper.CreateErrorResponse();
                    break;
            }

            Thread.Sleep(Delay);
            return Task.Factory.StartNew(() => retVal);
        }

        protected abstract IHttpResponseAbstraction HandleGet();

        protected abstract IHttpResponseAbstraction HandlePost();

        protected abstract IHttpResponseAbstraction HandlePut();

        protected abstract IHttpResponseAbstraction HandleDelete();

        protected abstract IHttpResponseAbstraction HandleHead();

        protected abstract IHttpResponseAbstraction HandleCopy();
    }
}
