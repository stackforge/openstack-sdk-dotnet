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

namespace OpenStack.Test.Functional
{
    /// <summary>
    /// Configuration used for testing.
    /// </summary>
    public static class Configuration
    {
        // Identity
        // This can be found here: https://horizon.hpcloud.com/project/access_and_security/
        public static Uri AuthUri = new Uri("https://XXXXXXXX.identity.hpcloudsvc.com:35357/v2.0/");
        public static string UserName = "username";
        public static string Password = "password";
        public static string TenantId = "the-tenant-id-here";
        
        // Storage tests
        // The storage service name can be found in the ServiceCatalog. It is different
        // for Public Cloud and private Helion OpenStack deployments.
        public static string StorageServiceName = "Object Storage";
        public static string ContainerName = "TestContainer";
        public static string FolderName = "TestFolder";
        public static string ObjectName = "TestObject";
    }
}
