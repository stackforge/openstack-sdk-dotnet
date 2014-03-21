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
using System.Threading.Tasks;
using Openstack.Common.Http;
using Openstack.Storage;

namespace Openstack.Test.Storage
{
    public class TestStorageServiceRestClient : IStorageServiceRestClient
    {
        public IHttpResponseAbstraction Response { get; set; }

        public Task<IHttpResponseAbstraction> CreateObject(string containerName, string objectName, IDictionary<string, string> metadata, Stream content)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> CreateContainer(string containerName, IDictionary<string, string> metadata)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> GetObject(string containerName, string objectName)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> GetContainer(string containerName)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> DeleteObject(string containerName, string objectName)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> DeleteContainer(string containerName)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> UpdateObject(string containerName, string objectName, IDictionary<string, string> metadata)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> UpdateContainer(string containerName, IDictionary<string, string> metadata)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> GetContainerMetadata(string containerName)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> GetObjectMetadata(string containerName, string objectName)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> CopyObject(string sourceContainerName, string sourceObjectName, string targetContainerName,
            string targetObjectName)
        {
            return Task.Factory.StartNew(() => Response);
        }

        public Task<IHttpResponseAbstraction> GetAccount()
        {
            return Task.Factory.StartNew(() => Response);
        }
    }

    public class TestStorageServiceRestClientFactory : IStorageServiceRestClientFactory
    {
        internal IStorageServiceRestClient Client;
        public TestStorageServiceRestClientFactory(IStorageServiceRestClient client)
        {
            this.Client = client;
        }

        public IStorageServiceRestClient Create(StorageServiceClientContext context)
        {
            return Client;
        }
    }
}
