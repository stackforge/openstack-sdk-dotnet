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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class StorageItemTests
    {
        [TestMethod]
        public void CanExtractNameWithNoSlashes()
        {
            var expectedFolderName = "myFolder";
            var folderName = StorageItem.ExtractName("myFolder");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithLeadingSlash()
        {
            var expectedFolderName = "myFolder";
            var folderName = StorageItem.ExtractName("/myFolder");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithTrailingSlash()
        {
            var expectedFolderName = "myFolder";
            var folderName = StorageItem.ExtractName("myFolder/");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithTrailingAndLeadingSlash()
        {
            var expectedFolderName = "myFolder";
            var folderName = StorageItem.ExtractName("/myFolder/");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithJustSlash()
        {
            var expectedFolderName = string.Empty;
            var folderName = StorageItem.ExtractName("/");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithDoubleLeadingSlash()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageItem.ExtractName("//MyFolder");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithDoubleTrailingSlash()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageItem.ExtractName("MyFolder//");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithDoubleTrailingAndLeadingSlashes()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageItem.ExtractName("//MyFolder//");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithMultipleFolders()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageItem.ExtractName("Folder1/Folder2/Folder3/MyFolder");
            Assert.AreEqual(expectedFolderName, folderName);
        }

        [TestMethod]
        public void CanExtractNameWithMultipleFoldersAndSlashes()
        {
            var expectedFolderName = "MyFolder";
            var folderName = StorageItem.ExtractName("Folder1//Folder2/Folder3//MyFolder/");
            Assert.AreEqual(expectedFolderName, folderName);
        }
    }
}
