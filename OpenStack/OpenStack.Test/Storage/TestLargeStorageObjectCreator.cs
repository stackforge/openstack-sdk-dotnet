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
using System.Threading.Tasks;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    public class TestLargeStorageObjectCreatorFactory : ILargeStorageObjectCreatorFactory
    {
        internal ILargeStorageObjectCreator creator;

        public TestLargeStorageObjectCreatorFactory(ILargeStorageObjectCreator creator)
        {
            this.creator = creator;
        }
        public ILargeStorageObjectCreator Create(IStorageServiceClient client)
        {
            return creator;
        }
    }

    public class TestLargeStorageObjectCreator : ILargeStorageObjectCreator
    {
        public Func<string, string, IDictionary<string, string>, Stream, int, string, Task<StorageObject>> CreateDelegate { get; set; }

        public async Task<StorageObject> Create(string containerName, string objectName, IDictionary<string, string> metadata, Stream content, int numberOfSegments,
            string segmentsContainer)
        {
            return
                await CreateDelegate(containerName, objectName, metadata, content, numberOfSegments, segmentsContainer);
        }
    }
}
