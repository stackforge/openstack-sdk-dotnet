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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class StorageFolderTests
    {
        [TestMethod]
        public void CanCreateFolder()
        {
            var folder = new StorageFolder("MyFolder", new List<StorageFolder>());
            Assert.AreEqual("MyFolder", folder.FullName);
            Assert.AreEqual("MyFolder", folder.Name);
            Assert.AreEqual(0,folder.Folders.Count());
        }

        [TestMethod]
        public void CanCreateFolderWithLeadingSlashes()
        {
            var expectedFullFolderName = "//MyFolder";
            var expectedFolderName = "MyFolder";
            var folder = new StorageFolder("//MyFolder", new List<StorageFolder>());
            Assert.AreEqual(expectedFullFolderName, folder.FullName);
            Assert.AreEqual(expectedFolderName, folder.Name);
            Assert.AreEqual(0, folder.Folders.Count());
        }

        [TestMethod]
        public void CanCreateFolderWithTrailingSlashes()
        {
            var expectedFullFolderName = "MyFolder//";
            var expectedFolderName = "MyFolder";
            var folder = new StorageFolder("MyFolder//", new List<StorageFolder>());
            Assert.AreEqual(expectedFullFolderName, folder.FullName);
            Assert.AreEqual(expectedFolderName, folder.Name);
            Assert.AreEqual(0, folder.Folders.Count());
        }

        [TestMethod]
        public void CanCreateFolderWithMultipleFoldersInName()
        {
            var expectedFolderName = "MyFolder";
            var folder = new StorageFolder("//Some/Folder/MyFolder", new List<StorageFolder>());
            Assert.AreEqual("//Some/Folder/MyFolder", folder.FullName);
            Assert.AreEqual(expectedFolderName, folder.Name);
            Assert.AreEqual(0, folder.Folders.Count());
        }
    }
}
