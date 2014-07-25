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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Common.Http;

namespace OpenStack.Test.Storage
{
    public class StorageRestSimulator : RestSimulator
    {
        internal class StorageItem
        {
            public string Name { get; set; }
            
            public IDictionary<string,string> MetaData { get; set; }

            public Stream Content { get; set; }

            public StorageItem()
            {
                this.MetaData = new Dictionary<string, string>();
            }

            public StorageItem(string name) : this()
            {
                this.Name = name;
            }

            public void ProcessMetaDataFromHeaders(IDictionary<string, string> headers)
            {
                headers.Keys.Where(k => k.ToLowerInvariant().StartsWith("x-object-meta-")).ToList().ForEach(i => this.MetaData.Add(i, headers[i]));
                headers.Keys.Where(k => k.ToLowerInvariant().StartsWith("x-container-meta-")).ToList().ForEach(i => this.MetaData.Add(i, headers[i]));
            }

            public void LoadContent(Stream content)
            {
                var memStream = new MemoryStream();
                if (content != null)
                {
                    content.CopyTo(memStream);
                    memStream.Position = 0;
                }
                this.Content = memStream;
            }
        }

        public StorageRestSimulator() : base()
        {
            this.Containers = new Dictionary<string, StorageItem>();
            this.Objects = new Dictionary<string, StorageItem>();
            this.IsContainerEmpty = true;
        }

        public StorageRestSimulator(CancellationToken token) : this()
        {
        }

        internal Dictionary<string, StorageItem> Containers { get; set; }

        internal Dictionary<string, StorageItem> Objects { get; set; }

        public bool IsContainerEmpty { get; set; }

        //public event EventHandler<HttpProgressEventArgs> HttpReceiveProgress;
        //public event EventHandler<HttpProgressEventArgs> HttpSendProgress;

        protected override IHttpResponseAbstraction HandleCopy()
        {
            var containerName = GetContainerName(this.Uri.Segments);
            var objectName = GetObjectName(this.Uri.Segments);

            if (containerName == null && objectName == null)
            {
                return TestHelper.CreateErrorResponse();
            }

            //cannot copy a container
            if (objectName == null)
            {
                return TestHelper.CreateResponse(HttpStatusCode.MethodNotAllowed);
            }

            if (!this.Containers.ContainsKey(containerName))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            if (!this.Headers.ContainsKey("Destination"))
            {
                return TestHelper.CreateResponse(HttpStatusCode.PreconditionFailed);
            }

            if (!this.Objects.ContainsKey(objectName))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            var destination = this.Headers["Destination"];
            var destinationSegs = destination.Split('/');
            if (destinationSegs.Count() < 2)
            {
                return TestHelper.CreateResponse(HttpStatusCode.PreconditionFailed);
            }

            if (destinationSegs[1] == string.Empty)
            {
                return TestHelper.CreateResponse(HttpStatusCode.MethodNotAllowed);
            }

            if (!this.Containers.ContainsKey(destinationSegs[0]))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            var destObjectName = string.Join("", destinationSegs.Skip(1));
            var srcObj = this.Objects[objectName];
            
            var obj = new StorageItem(destObjectName);
            obj.MetaData = srcObj.MetaData;
            obj.ProcessMetaDataFromHeaders(this.Headers);
            var content = new MemoryStream();
            srcObj.Content.CopyTo(content);
            srcObj.Content.Position = 0;
            content.Position = 0;
            obj.Content = content;

            this.Objects[obj.Name] = obj;

            var headers = GenerateObjectResponseHeaders(obj);

            return TestHelper.CreateResponse(HttpStatusCode.Created, headers);

        }

        protected override IHttpResponseAbstraction HandleHead()
        {
            var containerName = GetContainerName(this.Uri.Segments);
            var objectName = GetObjectName(this.Uri.Segments);

            if (containerName == null && objectName == null)
            {
                return TestHelper.CreateErrorResponse();
            }

            var headers = new Dictionary<string, string>();

            if (objectName != null)
            {
                if (!this.Objects.ContainsKey(objectName))
                {
                    return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                }
                headers = GenerateObjectResponseHeaders(this.Objects[objectName]);
            }
            else
            {
                if (!this.Containers.ContainsKey(containerName))
                {
                    return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                }
                headers = GenerateContainerResponseHeaders(this.Containers[containerName]);
            }

            return TestHelper.CreateResponse(HttpStatusCode.NoContent, headers);
        }

        protected override IHttpResponseAbstraction HandleDelete()
        {
            var containerName = GetContainerName(this.Uri.Segments);
            var objectName = GetObjectName(this.Uri.Segments);

            if (containerName == null && objectName == null)
            {
                return TestHelper.CreateErrorResponse();
            }

            if (objectName != null)
            {
                if (!this.Objects.ContainsKey(objectName))
                {
                    return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                }

                this.Objects.Remove(objectName);

                return TestHelper.CreateResponse(HttpStatusCode.NoContent, GenerateDeleteHeaders());
            }

            if (!this.Containers.ContainsKey(containerName))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            var statusCode = HttpStatusCode.NoContent;

            if (!this.IsContainerEmpty)
            {
                statusCode = HttpStatusCode.Conflict;
            }
            else
            {
                this.Containers.Remove(containerName);
            }

            return TestHelper.CreateResponse(statusCode, GenerateDeleteHeaders());
        }

        protected override IHttpResponseAbstraction HandlePut()
        {
            var containerName = GetContainerName(this.Uri.Segments);
            var objectName = GetObjectName(this.Uri.Segments);

            if (containerName == null && objectName == null)
            {
                return TestHelper.CreateErrorResponse();
            }

            if (objectName != null)
            {
                var obj = new StorageItem(objectName);
                obj.ProcessMetaDataFromHeaders(this.Headers);
                obj.LoadContent(this.Content);
                this.Objects[obj.Name] = obj;

                var headers = GenerateObjectResponseHeaders(obj);

                return TestHelper.CreateResponse(HttpStatusCode.Created, headers);
            }
            else
            {
                var container = new StorageItem(containerName);
                container.ProcessMetaDataFromHeaders(this.Headers);

                var containerUpdated = this.Containers.Keys.Any(k => String.Equals(k, container.Name, StringComparison.InvariantCultureIgnoreCase));

                this.Containers[container.Name] = container;

                return TestHelper.CreateResponse(containerUpdated ? HttpStatusCode.Accepted : HttpStatusCode.Created);
            }
        }

        protected override IHttpResponseAbstraction HandlePost()
        {
            var containerName = GetContainerName(this.Uri.Segments);
            var objectName = GetObjectName(this.Uri.Segments);

            if (containerName == null && objectName == null)
            {
                return TestHelper.CreateErrorResponse();
            }

            StorageItem storageItem;

            if (objectName != null)
            {
                if (!this.Objects.ContainsKey(objectName))
                {
                    return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                }

                storageItem = this.Objects[objectName];
            }
            else
            {
                if (!this.Containers.ContainsKey(containerName))
                {
                    return TestHelper.CreateResponse(HttpStatusCode.NotFound);
                }

                storageItem = this.Containers[containerName];
            }

            storageItem.MetaData.Clear();
            storageItem.ProcessMetaDataFromHeaders(this.Headers);

            return TestHelper.CreateResponse(HttpStatusCode.Accepted);
        }

        protected override IHttpResponseAbstraction HandleGet()
        {
            var containerName = GetContainerName(this.Uri.Segments);
            var objectName = GetObjectName(this.Uri.Segments);
            var accountName = GetAccountName(this.Uri.Segments);

            if (containerName == null && objectName == null)
            {
                if (accountName == null)
                {
                    return TestHelper.CreateErrorResponse();
                }

                var accountHeaders = GenerateAccountResponseHeaders();
                var accountContent = TestHelper.CreateStream(GenerateAccountPayload());

                return TestHelper.CreateResponse(HttpStatusCode.OK, accountHeaders, accountContent);

            }

            if (objectName != null)
            {
                return this.GetObject(objectName);
            }

            var query = this.Uri.ParseQueryString();
            if (query.HasKeys() && query["prefix"] != null && query["delimiter"] != null)
            {
                return this.GetObject(query["prefix"]);
            }
           
            if (!this.Containers.ContainsKey(containerName))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            var container = this.Containers[containerName];
            var headers = GenerateContainerResponseHeaders(container);
            var content = TestHelper.CreateStream("[]");

            return TestHelper.CreateResponse(HttpStatusCode.OK, headers, content);
        }

        public IHttpResponseAbstraction GetObject(string objectName)
        {
            if (!this.Objects.ContainsKey(objectName))
            {
                return TestHelper.CreateResponse(HttpStatusCode.NotFound);
            }

            var obj = this.Objects[objectName];
            var objectHeaders = GenerateObjectResponseHeaders(obj);

            return TestHelper.CreateResponse(HttpStatusCode.OK, objectHeaders, obj.Content);
        }

        public string GetContainerName(string[] uriSegments)
        {
            return uriSegments.Count() < 4 ? null : uriSegments[3].TrimEnd('/');
        }

        public string GetAccountName(string[] uriSegments)
        {
            return uriSegments.Count() < 3 ? null : uriSegments[2].TrimEnd('/');
        }

        public string GetObjectName(string[] uriSegments)
        {
            if (uriSegments.Count() < 5)
            {
                return null;
            }
            return string.Join("", uriSegments.Skip(4));
        }

        private string GenerateAccountPayload()
        {
            var payload = new StringBuilder();
            payload.Append("[");
            var first = true;
            foreach (var container in this.Containers)
            {
                if (!first)
                {
                    payload.Append(",");
                    first = false;
                }

                payload.Append("{");
                payload.Append("\"count\": 42,");
                payload.Append("\"bytes\": 12345,");
                payload.Append(string.Format("\"name\": \"{0}\"",container.Value.Name));
                payload.Append("}");
            }
            payload.Append("]");
            return payload.ToString();
        }

        private IEnumerable<KeyValuePair<string, string>> GenerateAccountResponseHeaders()
        {
           return new Dictionary<string, string>()
            {
                {"X-Account-Bytes-Used", "12345"},
                {"X-Account-Container-Count", this.Containers.Count.ToString()},
                {"X-Account-Object-Count", this.Objects.Count.ToString()},
                {"Content-Type", this.ContentType},
                {"X-Trans-Id", "12345"},
                {"Date", DateTime.UtcNow.ToShortTimeString()},
                {"X-Timestamp", "1234567890.98765"}
            };
        }

        internal Dictionary<string, string> GenerateObjectResponseHeaders(StorageItem obj)
        {
            var etag = Convert.ToBase64String(MD5.Create().ComputeHash(obj.Content));
            obj.Content.Position = 0;

            return new Dictionary<string, string>(obj.MetaData)
            {
                {"ETag", etag},
                {"Content-Type", this.ContentType},
                {"X-Trans-Id", "12345"},
                {"Date", DateTime.UtcNow.ToShortTimeString()},
                {"X-Timestamp", "1234567890.98765"}
            };
        }

        internal Dictionary<string, string> GenerateContainerResponseHeaders(StorageItem obj)
        {
            return new Dictionary<string, string>(obj.MetaData)
            {
                {"X-Container-Bytes-Used", "0"},
                {"Content-Type", this.ContentType},
                {"X-Container-Object-Count", "0"},
                {"Date", DateTime.UtcNow.ToShortTimeString()},
                {"X-Container-Read", ".r.*,.rlistings"},
                {"X-Trans-Id", "12345"},
                {"X-Timestamp", "1234567890.98765"}
            };
        }

        internal Dictionary<string, string> GenerateDeleteHeaders()
        {
            return new Dictionary<string, string>()
            {
                {"X-Trans-Id", "12345"},
                {"Date", DateTime.UtcNow.ToShortTimeString()}
            };
        }
    }

    public class StorageRestSimulatorFactory : IHttpAbstractionClientFactory
    {
        internal StorageRestSimulator Simulator = null;
        public StorageRestSimulatorFactory(StorageRestSimulator simulator)
        {
            this.Simulator = simulator;
        }

        public IHttpAbstractionClient Create()
        {
            throw new NotImplementedException();
        }

        public IHttpAbstractionClient Create(CancellationToken token)
        {
            if (this.Simulator != null)
            {
                this.Simulator.Headers.Clear();
            }
            return this.Simulator ?? new StorageRestSimulator(token);
        }

        public IHttpAbstractionClient Create(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IHttpAbstractionClient Create(TimeSpan timeout, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
