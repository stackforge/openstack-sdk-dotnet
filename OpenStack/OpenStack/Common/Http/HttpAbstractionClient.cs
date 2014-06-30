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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStack.Common.Http
{
    public class HttpAbstractionClient : DisposableClass, IHttpAbstractionClient
    {
        private readonly HttpClient _client;
        private CancellationToken _cancellationToken = CancellationToken.None;
        private static readonly TimeSpan _defaultTimeout = new TimeSpan(0, 5, 0);

        internal HttpAbstractionClient(TimeSpan timeout, CancellationToken cancellationToken)
        {
            this._client = new HttpClient(new HttpClientHandler()) { Timeout = timeout };

            this._cancellationToken = cancellationToken;
            this.Method = HttpMethod.Get;
            this.Headers = new Dictionary<string, string>();
            this.ContentType = string.Empty;
        }

        public static IHttpAbstractionClient Create()
        {
            return new HttpAbstractionClient(_defaultTimeout, CancellationToken.None );
        }

        public static IHttpAbstractionClient Create(TimeSpan timeout)
        {
            return new HttpAbstractionClient(timeout, CancellationToken.None);
        }

        public static IHttpAbstractionClient Create(CancellationToken cancellationToken)
        {
            return new HttpAbstractionClient(_defaultTimeout, cancellationToken);
        }

        public static IHttpAbstractionClient Create(CancellationToken cancellationToken, TimeSpan timeout)
        {
            return new HttpAbstractionClient(timeout, cancellationToken);
        }

        public HttpMethod Method { get; set; }

        public Uri Uri { get; set; }

        public Stream Content { get; set; }

        public IDictionary<string, string> Headers { get; private set; }

        public string ContentType { get; set; }

        public TimeSpan Timeout { get; set; }

        public async Task<IHttpResponseAbstraction> SendAsync()
        {
            var requestMessage = new HttpRequestMessage { Method = this.Method, RequestUri = this.Uri };

            if (this.Method == HttpMethod.Post || this.Method == HttpMethod.Put)
            {
                if (this.Content != null)
                {
                    requestMessage.Content = new StreamContent(this.Content);
                    if (this.ContentType != string.Empty)
                    {
                        requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(this.ContentType);
                    }
                }
            }

            requestMessage.Headers.Clear();
            foreach (var header in this.Headers)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }

            var startTime = DateTime.Now;
            
            try
            {
                
                var result = await this._client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, this._cancellationToken);

                var headers = new HttpHeadersAbstraction(result.Headers);

                Stream content = null;
                if (result.Content != null )
                {
                    headers.AddRange(result.Content.Headers);
                    content = this.WaitForResult(result.Content.ReadAsStreamAsync(), new TimeSpan(0,0,0,0,int.MaxValue) );
                }

                var retval = new HttpResponseAbstraction(content, headers, result.StatusCode);

                //TODO: Add logging code
                
                return retval;
            }
            catch (Exception ex)
            {
                //TODO: Add logging code

                var tcex = ex as TaskCanceledException;
                if (tcex == null)
                {
                    throw;
                }

                if (this._cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("The operation was canceled by user request.", tcex, this._cancellationToken);
                }
                
                if (DateTime.Now - startTime > this.Timeout)
                {
                    throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "The task failed to complete in the given timeout period ({0}).", this.Timeout));
                }

                throw;
            }
        }

        internal T WaitForResult<T>(Task<T> task, TimeSpan timeout)
        {
            if (task == null )
            {
                throw new ArgumentNullException("task");
            }

            try
            {
                task.Wait(timeout);
            }
            catch (AggregateException aggregateException)
            {
                throw GetInnerException(aggregateException);
            }

            if (task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Faulted && task.Status != TaskStatus.Canceled)
            {
                throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "The task failed to complete in the given timeout period ({0}).", timeout));
            }

            return task.Result;
        }

        internal Exception GetInnerException(AggregateException aggregateException)
        {
            //helper function to spool off the layers of aggregate exceptions and get the underlying exception... 
            Exception innerExcception = aggregateException;
            while (aggregateException != null)
            {
                innerExcception = aggregateException.InnerExceptions[0];
                aggregateException = innerExcception as AggregateException;
            }
            return innerExcception;
        }
    }
}
