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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class StorageFolderPayloadConverterTests
    {
        [TestMethod]
        public void CanAddFolderWithNestedFolders()
        {
            var objects = new List<StorageObject>() { new StorageObject("a/b/c/d/","a") };
            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var folders = converter.Convert(objects).ToList();
            
            Assert.AreEqual(1,folders.Count);
            Assert.AreEqual("a",folders[0].Name);
            Assert.AreEqual(1, folders[0].Folders.Count);
            Assert.AreEqual("b", folders[0].Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.Count);
            Assert.AreEqual("c", folders[0].Folders.First().Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.First().Folders.Count);
            Assert.AreEqual("d", folders[0].Folders.First().Folders.First().Folders.First().Name);
        }

        [TestMethod]
        public void CanAddFolderWithNestedFoldersAndDuplicateNames()
        {
            var objects = new List<StorageObject>() { new StorageObject("a/c/c/c/", "a") };
            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var folders = converter.Convert(objects).ToList();

            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual("a", folders[0].Name);
            Assert.AreEqual(1, folders[0].Folders.Count);
            Assert.AreEqual("c", folders[0].Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.Count);
            Assert.AreEqual("c", folders[0].Folders.First().Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.First().Folders.Count);
            Assert.AreEqual("c", folders[0].Folders.First().Folders.First().Folders.First().Name);
        }

        [TestMethod]
        public void CanAddSingleFolder()
        {
            var objects = new List<StorageObject>() { new StorageObject("a/", "a") };
            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var folders = converter.Convert(objects).ToList();

            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual("a", folders[0].Name);
            Assert.AreEqual(0, folders[0].Folders.Count);
        }

        [TestMethod]

        [ExpectedException(typeof(ArgumentNullException))]
        public void CanAddFolderWithNullObjectList()
        {
            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            converter.Convert(null);
        }

        [TestMethod]
        public void CanAddNestedFoldersWhenRootExists()
        {
            var objects = new List<StorageObject>() { new StorageObject("a/", "a"), new StorageObject("a/b/c/d/", "a") };

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var folders = converter.Convert(objects).ToList();

            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual("a", folders[0].Name);
            Assert.AreEqual(1, folders[0].Folders.Count);
            Assert.AreEqual("b", folders[0].Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.Count);
            Assert.AreEqual("c", folders[0].Folders.First().Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.First().Folders.Count);
            Assert.AreEqual("d", folders[0].Folders.First().Folders.First().Folders.First().Name);
        }

        [TestMethod]
        public void CanAddNestedFoldersWhenRootExistsWithObjectsAtLeaf()
        {
            var objects = new List<StorageObject>() { new StorageObject("a/b/c/d/foo", "a"), new StorageObject("a/b/c/d/bar", "a") };

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var folders = converter.Convert(objects).ToList();
            
            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual("a", folders[0].Name);
            Assert.AreEqual(1, folders[0].Folders.Count);
            Assert.AreEqual("b", folders[0].Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.Count);
            Assert.AreEqual("c", folders[0].Folders.First().Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.First().Folders.Count);
            
            var leaf = folders[0].Folders.First().Folders.First().Folders.First();
            Assert.AreEqual("d", leaf.Name);
            Assert.AreEqual(2,leaf.Objects.Count);
            Assert.IsTrue(leaf.Objects.Any(o => o.FullName == "a/b/c/d/foo"));
            Assert.IsTrue(leaf.Objects.Any(o => o.FullName == "a/b/c/d/bar"));
        }

        [TestMethod]
        public void CanAddNestedFoldersWhenRootExistsWithObjectsAtLeafAndRoot()
        {
            var objects = new List<StorageObject>() { new StorageObject("a/b/c/d/foo", "a"), new StorageObject("a/b/c/d/bar", "a"), new StorageObject("xyz", "a") };

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var folders = converter.Convert(objects).ToList();

            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual("a", folders[0].Name);
            Assert.AreEqual(1, folders[0].Folders.Count);
            Assert.AreEqual("b", folders[0].Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.Count);
            Assert.AreEqual("c", folders[0].Folders.First().Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.First().Folders.Count);

            var leaf = folders[0].Folders.First().Folders.First().Folders.First();
            Assert.AreEqual("d", leaf.Name);
            Assert.AreEqual(2, leaf.Objects.Count);
            Assert.IsTrue(leaf.Objects.Any(o => o.FullName == "a/b/c/d/foo"));
            Assert.IsTrue(leaf.Objects.Any(o => o.FullName == "a/b/c/d/bar"));
        }

        [TestMethod]
        public void CanAddNestedFoldersWhenRootExistsWithObjectsAtLeafAndLongNameAtRoot()
        {
            var objects = new List<StorageObject>() { new StorageObject("a/b/c/d/foo", "a"), new StorageObject("a/b/c/d/bar", "a"), new StorageObject("thiswillsorttothetopofthelist", "a") };

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var folders = converter.Convert(objects).ToList();

            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual("a", folders[0].Name);
            Assert.AreEqual(1, folders[0].Folders.Count);
            Assert.AreEqual("b", folders[0].Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.Count);
            Assert.AreEqual("c", folders[0].Folders.First().Folders.First().Name);
            Assert.AreEqual(1, folders[0].Folders.First().Folders.First().Folders.Count);

            var leaf = folders[0].Folders.First().Folders.First().Folders.First();
            Assert.AreEqual("d", leaf.Name);
            Assert.AreEqual(2, leaf.Objects.Count);
            Assert.IsTrue(leaf.Objects.Any(o => o.FullName == "a/b/c/d/foo"));
            Assert.IsTrue(leaf.Objects.Any(o => o.FullName == "a/b/c/d/bar"));
        }

        [TestMethod]
        public void CanAddNestedFoldersWhenRootExistsWithObjectsAtManyLevels()
        {
            var objects = new List<StorageObject>() { new StorageObject("a/b/c/d/foo", "a"), 
                new StorageObject("a/b/bar", "a"), 
                new StorageObject("a/b/c/beans", "a"), 
                new StorageObject("a/string", "a"),
                new StorageObject("a/b/c/d/", "a") ,
                new StorageObject("a/b/c/", "a") ,
                new StorageObject("a/", "a") 
            };

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var folders = converter.Convert(objects).ToList();

            var aNode = folders[0];
            var bNode = folders[0].Folders.First();
            var cNode = folders[0].Folders.First().Folders.First();
            var dNode = folders[0].Folders.First().Folders.First().Folders.First();

            Assert.AreEqual(1, folders.Count);

            Assert.AreEqual("a", aNode.Name);
            Assert.AreEqual(1, aNode.Folders.Count);
            Assert.AreEqual(1, aNode.Objects.Count);
            Assert.IsTrue(aNode.Objects.Any(o => o.FullName == "a/string"));

            Assert.AreEqual("b", bNode.Name);
            Assert.AreEqual("a/b", bNode.FullName);
            Assert.AreEqual(1, bNode.Folders.Count);
            Assert.AreEqual(1, bNode.Objects.Count);
            Assert.IsTrue(bNode.Objects.Any(o => o.FullName == "a/b/bar"));

            Assert.AreEqual("c", cNode.Name);
            Assert.AreEqual("a/b/c", cNode.FullName);
            Assert.AreEqual(1, cNode.Folders.Count);
            Assert.AreEqual(1, cNode.Objects.Count);
            Assert.IsTrue(cNode.Objects.Any(o => o.FullName == "a/b/c/beans"));

            Assert.AreEqual("d", dNode.Name);
            Assert.AreEqual("a/b/c/d", dNode.FullName);
            Assert.AreEqual(1, dNode.Objects.Count);
            Assert.IsTrue(dNode.Objects.Any(o => o.FullName == "a/b/c/d/foo"));
        }

        [TestMethod]
        public void CanConvertFoldersWithObjects()
        {
            var objects = new List<StorageObject>()
            {
                new StorageObject("a", "a"),
                new StorageObject("a/", "a"),
                new StorageObject("b//", "a"),
                new StorageObject("//a/", "a"),
                new StorageObject("a/b/", "a", DateTime.Now, "12345", 100, "application/directory")
            };

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var resp = converter.Convert(objects).ToList();

            Assert.AreEqual(1, resp.Count);

            var aNode = resp.First();
            Assert.AreEqual("a", aNode.Name);
            Assert.AreEqual(1, aNode.Folders.Count);
            Assert.AreEqual(0, aNode.Objects.Count);

            var bNode = aNode.Folders.First();
            Assert.AreEqual("b", bNode.Name);
            Assert.AreEqual("a/b", bNode.FullName);
            Assert.AreEqual(0, bNode.Folders.Count);
            Assert.AreEqual(0, bNode.Objects.Count);

            Assert.AreEqual(5, objects.Count);
        }

        [TestMethod]
        public void CanConvertFoldersWithNoInputObjects()
        {
            var objects = new List<StorageObject>();

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var resp = converter.Convert(objects).ToList();

            Assert.AreEqual(0, resp.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotConvertFoldersWithNullObjectList()
        {
            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
           var resp = converter.Convert(null);
        }

        [TestMethod]
        public void CanConvertFolderWithValidJsonAndNoSubFoldersOrFolderObject()
        {
            var containerName = "container";
            var folderName = "a/b/c/";
            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var resp = converter.Convert(containerName, folderName, payload);

            Assert.AreEqual(1, resp.Objects.Count);
            Assert.AreEqual(0, resp.Folders.Count);
            Assert.AreEqual("a/b/c/", resp.FullName);
            Assert.AreEqual("c", resp.Name);

            var obj = resp.Objects.First();
            Assert.AreEqual("a/b/c/BLAH", obj.FullName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CannotConvertEmptyJsonArrayPayload()
        {
            var containerName = "container";
            var folderName = "a/b/c/";
            var payload = @"[]";

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            converter.Convert(containerName, folderName, payload);
        }

        [TestMethod]
        public void CanConvertFolderWithValidJsonFolderObjectAndNoSubFolders()
        {
            var containerName = "container";
            var folderName = "a/b/c/";
            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/"",
                                    ""content_type"": ""application/octet-stream""
                                },
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                }]";

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var resp = converter.Convert(containerName, folderName, payload);

            Assert.AreEqual(1, resp.Objects.Count);
            Assert.AreEqual(0, resp.Folders.Count);
            Assert.AreEqual("a/b/c/", resp.FullName);
            Assert.AreEqual("c", resp.Name);

            var obj = resp.Objects.First();
            Assert.AreEqual("a/b/c/BLAH", obj.FullName);
        }

        [TestMethod]
        public void CanConvertFolderWithValidJsonSubFoldersAndNoFolderObject()
        {
            var containerName = "container";
            var folderName = "a/b/c/";
            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                },
                                {
                                        ""subdir"": ""a/b/c/d/""
                                },
                                {
                                        ""subdir"": ""a/b/c/x/""
                                }]";

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var resp = converter.Convert(containerName, folderName, payload);

            Assert.AreEqual(1, resp.Objects.Count);
            Assert.AreEqual(2, resp.Folders.Count);
            Assert.AreEqual("a/b/c/", resp.FullName);
            Assert.AreEqual("c", resp.Name);

            var obj = resp.Objects.First();
            Assert.AreEqual("a/b/c/BLAH", obj.FullName);

            var dNode = resp.Folders.First(f => f.FullName == "a/b/c/d/");
            var xNode = resp.Folders.First(f => f.FullName == "a/b/c/x/");

            Assert.AreEqual("d", dNode.Name);
            Assert.AreEqual(0, dNode.Folders.Count);
            Assert.AreEqual(0, dNode.Objects.Count);

            Assert.AreEqual("x", xNode.Name);
            Assert.AreEqual(0, xNode.Folders.Count);
            Assert.AreEqual(0, xNode.Objects.Count);
        }

        [TestMethod]
        public void CanConvertFolderWithValidJsonFolderObjectAndSubFolders()
        {
            var containerName = "container";
            var folderName = "a/b/c/";
            var payload = @"[
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/"",
                                    ""content_type"": ""application/octet-stream""
                                },
                                {
                                    ""hash"": ""d41d8cd98f00b204e9800998ecf8427e"",
                                    ""last_modified"": ""2014-03-07T21:31:31.588170"",
                                    ""bytes"": 0,
                                    ""name"": ""a/b/c/BLAH"",
                                    ""content_type"": ""application/octet-stream""
                                },
                                {
                                        ""subdir"": ""a/b/c/d/""
                                },
                                {
                                        ""subdir"": ""a/b/c/x/""
                                }
                            ]";

            var converter = new StorageFolderPayloadConverter(new ServiceLocator());
            var resp = converter.Convert(containerName, folderName, payload);

            Assert.AreEqual("c", resp.Name);
            Assert.AreEqual("a/b/c/", resp.FullName);
            Assert.AreEqual(1, resp.Objects.Count);
            Assert.AreEqual(2, resp.Folders.Count);

            var obj = resp.Objects.First();
            Assert.AreEqual("a/b/c/BLAH", obj.FullName);

            var dNode = resp.Folders.First(f => f.FullName == "a/b/c/d/");
            var xNode = resp.Folders.First(f => f.FullName == "a/b/c/x/");

            Assert.AreEqual("d", dNode.Name);
            Assert.AreEqual(0, dNode.Folders.Count);
            Assert.AreEqual(0, dNode.Objects.Count);

            Assert.AreEqual("x", xNode.Name);
            Assert.AreEqual(0, xNode.Folders.Count);
            Assert.AreEqual(0, xNode.Objects.Count);
        }
    }
}
