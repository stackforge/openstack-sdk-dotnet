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

namespace Openstack.Test.Storage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Openstack.Storage;

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
            var expectedFolderName = "MyFolder";
            var folder = new StorageFolder("//MyFolder", new List<StorageFolder>());
            Assert.AreEqual(expectedFolderName, folder.FullName);
            Assert.AreEqual(expectedFolderName, folder.Name);
            Assert.AreEqual(0, folder.Folders.Count());
        }

        [TestMethod]
        public void CanCreateFolderWithTrailingSlashes()
        {
            var expectedFolderName = "MyFolder";
            var folder = new StorageFolder("MyFolder//", new List<StorageFolder>());
            Assert.AreEqual(expectedFolderName, folder.FullName);
            Assert.AreEqual(expectedFolderName, folder.Name);
            Assert.AreEqual(0, folder.Folders.Count());
        }

        [TestMethod]
        public void CanCreateFolderWithMultipleFoldersInName()
        {
            var expectedFolderName = "MyFolder";
            var folder = new StorageFolder("//Some/Folder/MyFolder", new List<StorageFolder>());
            Assert.AreEqual("Some/Folder/MyFolder", folder.FullName);
            Assert.AreEqual(expectedFolderName, folder.Name);
            Assert.AreEqual(0, folder.Folders.Count());
        }

        [TestMethod]
        public void CanExtractFolderNameWithNoSlashes()
        {
            var expectedFolderName = "myFolder";
            var folderName = StorageFolder.ExtractFolderName("myFolder");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithLeadingSlash()
        {
            var expectedFolderName = "myFolder";
            var folderName = StorageFolder.ExtractFolderName("/myFolder");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithTrailingSlash()
        {
            var expectedFolderName = "myFolder";
            var folderName = StorageFolder.ExtractFolderName("myFolder/");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithTrailingAndLeadingSlash()
        {
            var expectedFolderName = "myFolder";
            var folderName = StorageFolder.ExtractFolderName("/myFolder/");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithJustSlash()
        {
            var expectedFolderName = string.Empty;
            var folderName = StorageFolder.ExtractFolderName("/");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithDoubleLeadingSlash()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageFolder.ExtractFolderName("//MyFolder");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithDoubleTrailingSlash()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageFolder.ExtractFolderName("MyFolder//");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithDoubleTrailingAndLeadingSlashes()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageFolder.ExtractFolderName("//MyFolder//");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithMultipleFolders()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageFolder.ExtractFolderName("Folder1/Folder2/Folder3/MyFolder");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractFolderNameWithMultipleFoldersAndSlashes()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageFolder.ExtractFolderName("Folder1//Folder2/Folder3//MyFolder/");
            Assert.AreEqual(expectedFolderName, folderName);
        }
    }
}
