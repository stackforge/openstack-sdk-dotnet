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

namespace OpenStack.Common.Http
{
    public interface IHttpHeadersAbstraction : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        void Add(string name, IEnumerable<string> values);

        void Add(string name, string value);

        void Clear();

        bool Contains(string name);

        IEnumerable<string> GetValues(string name);

        void Remove(string name);

        bool TryGetValue(string name, out IEnumerable<string> values);

        IEnumerable<string> this[string name] { get; set; }
    }
}
