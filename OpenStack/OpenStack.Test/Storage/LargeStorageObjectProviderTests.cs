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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common;
using OpenStack.Storage;

namespace OpenStack.Test.Storage
{
    [TestClass]
    public class LargeStorageObjectProviderTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetChunkSizeWithZeroSize()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            provider.GetChunkSize(0, 10);
        }

        [TestMethod]
        public void GetChunkSizeWithZeroSegments()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var size = provider.GetChunkSize(100, 0);

            //If no segments are requested, then the whole length should be returned.
            Assert.AreEqual(100, size);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetChunkSizeWithNegativeLength()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            provider.GetChunkSize(-1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetChunkSizeWithNegativeSegments()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            provider.GetChunkSize(100, -1);
        }

        [TestMethod]
        public void GetChunkSizeWithMoreSegmentsThenLength()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var size = provider.GetChunkSize(10, 100);

            //If the chunk size is calculated to less then zero, it should be rounded up to 1
            Assert.AreEqual(1, size);
        }

        [TestMethod]
        public void GetChunkSizeWithOneSegment()
        {
            var fileLength = 100;
            var segments = 1;
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var size = provider.GetChunkSize(fileLength, segments);

            //If there is just one segment, then the size should be the whole length
            Assert.AreEqual(fileLength, size);
        }

        [TestMethod]
        public void GetChunkSizeWithEvenSegmentSplit()
        {
            var fileLength = 100;
            var segments = 10;
            var expectedSize = 10;

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var size = provider.GetChunkSize(fileLength, segments);

            //If there is just one segment, then the size should be the whole length
            Assert.AreEqual(expectedSize, size);
        }

        [TestMethod]
        public void GetChunkSizeWithUnevenSplitLessThenHalf()
        {
            var fileLength = 103;
            var segments = 10;
            var expectedSize = 11;

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var size = provider.GetChunkSize(fileLength, segments);

            //Chunk size should be rounded up, in this case 103 / 10 = 10.3 -> 11
            Assert.AreEqual(expectedSize, size);
        }

        [TestMethod]
        public void GetChunkSizeWithUnevenSplitMoreThenHalf()
        {
            var fileLength = 107;
            var segments = 10;
            var expectedSize = 11;

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var size = provider.GetChunkSize(fileLength, segments);

            //Chunk size should be rounded up, in this case 107 / 10 = 10.7 -> 11
            Assert.AreEqual(expectedSize, size);
        }

        [TestMethod]
        public void GetSegmentIdFromKeyWithValidFormat()
        {
            var segment = 1234567890;
            var sObject = new StorageObject(segment.ToString("D10"),"TestContainer",new Dictionary<string, string>());

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segmentId = provider.GetSegmentIdFromKey(sObject);

            Assert.IsNotNull(segmentId);
            Assert.AreEqual(segmentId, segmentId);
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void GetSegmentIdFromKeyWithTrailingPeriod()
        {
            var segment = 1234567890;
            var sObject = new StorageObject(segment.ToString("D10") +".", "TestContainer", new Dictionary<string, string>());

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.GetSegmentIdFromKey(sObject);
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void GetSegmentIdFromKeyWithNonNumberText()
        {
            var segment = 1234567890;
            var sObject = new StorageObject("File.txt." + segment.ToString("D7") + "BAD", "TestContainer", new Dictionary<string, string>());

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.GetSegmentIdFromKey(sObject);
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void GetSegmentIdFromKeyWithWayTooBigNumber()
        {
            var segment = 123456789012345;
            var sObject = new StorageObject(segment.ToString("D15"), "TestContainer", new Dictionary<string, string>());

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.GetSegmentIdFromKey(sObject);
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void GetSegmentIdFromKeyMaxInt32PlusOne()
        {
            long segment = Int32.MaxValue;
            segment++;

            var sObject = new StorageObject(segment.ToString("D10"), "TestContainer", new Dictionary<string, string>());

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.GetSegmentIdFromKey(sObject);
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void GetSegmentIdFromKeyWithNegativeSegmentNumber()
        {
            long segment = -12345;

            var sObject = new StorageObject(segment.ToString("D10"), "TestContainer", new Dictionary<string, string>());

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.GetSegmentIdFromKey(sObject);
        }

        [TestMethod]
        public void GetLastSegmentIdAndObjectWithNoObjects()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var segmentFolder = new StorageFolder("myFile", new List<StorageFolder>(), new List<StorageObject>());

            var segment = provider.GetLastSegmentIdAndName(segmentFolder);

            Assert.IsNotNull(segment);
            Assert.AreEqual(0, segment.Key);
            Assert.AreEqual(string.Empty, segment.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetLastSegmentIdAndObjectWithNullFolder()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            provider.GetLastSegmentIdAndName(null);
        }

        [TestMethod]
        public void GetLastSegmentIdAndObjectWithOneObjectWithIdZero()
        {
            var sObject = new StorageObject("0000000000", "TestContainer", new Dictionary<string, string>());
            var segmentFolder = new StorageFolder("myFile", new List<StorageFolder>(), new List<StorageObject>() { sObject });

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segment = provider.GetLastSegmentIdAndName(segmentFolder);

            Assert.IsNotNull(segment);
            Assert.AreEqual(0, segment.Key);
            Assert.IsNotNull(segment.Value);
            Assert.AreEqual(sObject.Name, segment.Value);
        }

        [TestMethod]
        public void GetLastSegmentIdAndObjectWithOneObjectWithNonZeroId()
        {
            var segmentId = 1234567890;
            var sObject = new StorageObject(segmentId.ToString("D10"), "TestContainer", new Dictionary<string, string>());
            var segmentFolder = new StorageFolder("myFile", new List<StorageFolder>(), new List<StorageObject>() { sObject });

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segment = provider.GetLastSegmentIdAndName(segmentFolder);

            Assert.IsNotNull(segment);
            Assert.AreEqual(segmentId, segment.Key);
            Assert.IsNotNull(segment.Value);
            Assert.AreEqual(sObject.Name, segment.Value);
        }

        [TestMethod]
        public void GetLastSegmentIdAndObjectWithMultipleObjects()
        {
            var segmentOne = 0;
            var segmentTwo = 1;
            var sObjectOne = new StorageObject(segmentOne.ToString("D10"), "TestContainer", new Dictionary<string, string>());
            var sObjectTwo = new StorageObject(segmentTwo.ToString("D10"), "TestContainer", new Dictionary<string, string>());
            var segmentFolder = new StorageFolder("myFile", new List<StorageFolder>(), new List<StorageObject>() { sObjectOne, sObjectTwo });

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segment = provider.GetLastSegmentIdAndName(segmentFolder);

            Assert.IsNotNull(segment);
            Assert.AreEqual(segmentTwo, segment.Key);
            Assert.IsNotNull(segment.Value);
            Assert.AreEqual(sObjectTwo.Name, segment.Value);
        }

        [TestMethod]
        public void GetLastSegmentIdAndObjectWithMultipleObjectsOutOfOrder()
        {
            var segmentOne = 0;
            var segmentTwo = 1;
            var sObjectOne = new StorageObject(segmentOne.ToString("D10"), "TestContainer", new Dictionary<string, string>());
            var sObjectTwo = new StorageObject(segmentTwo.ToString("D10"), "TestContainer", new Dictionary<string, string>());
            var segmentFolder = new StorageFolder("myFile", new List<StorageFolder>(), new List<StorageObject>() { sObjectTwo, sObjectOne });

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segment = provider.GetLastSegmentIdAndName(segmentFolder);

            Assert.IsNotNull(segment);
            Assert.AreEqual(segmentTwo, segment.Key);
            Assert.IsNotNull(segment.Value);
            Assert.AreEqual(sObjectTwo.Name, segment.Value);
        }

        [TestMethod]
        public void GetLastSegmentIdAndObjectWithMultipleObjectsOutOfOrderAndAcrossAlphaSortBoundry()
        {
            var segmentOne = 1;
            var segmentTen = 10;
            var segmentTwo = 2;
            var sObjectOne = new StorageObject(segmentOne.ToString("D10"), "TestContainer", new Dictionary<string, string>());
            var sObjectTwo = new StorageObject(segmentTwo.ToString("D10"), "TestContainer", new Dictionary<string, string>());
            var sObjectTen = new StorageObject(segmentTen.ToString("D10"), "TestContainer", new Dictionary<string, string>());
            var segmentFolder = new StorageFolder("myFile", new List<StorageFolder>(), new List<StorageObject>() { sObjectTwo, sObjectTen, sObjectOne });

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segment = provider.GetLastSegmentIdAndName(segmentFolder);

            Assert.IsNotNull(segment);
            Assert.AreEqual(segmentTen, segment.Key);
            Assert.IsNotNull(segment.Value);
            Assert.AreEqual(sObjectTen.Name, segment.Value);
        }

        [TestMethod]
        public void GetLastSegmentIdAndObjectWithOneObjectWithBadSegmentId()
        {
            var sObjectTen = new StorageObject("THIS IS NOT A NUMBER", "TestContainer", new Dictionary<string, string>());
            var segmentFolder = new StorageFolder("myFile", new List<StorageFolder>(), new List<StorageObject>() {sObjectTen});

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segment = provider.GetLastSegmentIdAndName(segmentFolder);

            Assert.IsNotNull(segment);
            Assert.AreEqual(0, segment.Key);
            Assert.AreEqual(string.Empty, segment.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileChunkWithNegativeChunkSize()
        {
            long chunkSize = -10;
            var stream = new MemoryStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.GetObjectChunk(stream, chunkSize);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFileChunkWithNullStream()
        {
            long chunkSize = 100;
            Stream stream = null;

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.GetObjectChunk(stream, chunkSize);
        }

        [TestMethod]
        public void GetFileChunkWithEmptyStream()
        {
            long chunkSize = 100;
            Stream stream = new MemoryStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var chunk = provider.GetObjectChunk(stream, chunkSize);

            Assert.IsNotNull(chunk);
            Assert.AreEqual(chunkSize, chunk.Length);
            var memChunk = chunk as MemoryStream;
            Assert.IsTrue(memChunk.ToArray().All(b => b == default(Byte)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetFileChunkWithClosedStream()
        {
            long chunkSize = 100;
            var str = "some random data";
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            Stream stream = new MemoryStream(bytes);
            stream.Close();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.GetObjectChunk(stream, chunkSize);
        }

        [TestMethod]
        public void GetFileChunkWithValidStreamAndChunkSize()
        {
            long chunkSize = 10;
            var str = "some random data";
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            Stream stream = new MemoryStream(bytes);

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var chunk = provider.GetObjectChunk(stream, chunkSize);

            Assert.IsNotNull(chunk);
            Assert.AreEqual(chunkSize, chunk.Length);
            Assert.AreEqual(chunkSize, stream.Position);
            Assert.IsTrue(stream.CanRead);
        }

        [TestMethod]
        public void GetFileChunkWithValidStreamStopsOnEOF()
        {
            long chunkSize = 1000;
            var str = "some random data";
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            Stream stream = new MemoryStream(bytes);

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var chunk = provider.GetObjectChunk(stream, chunkSize);

            Assert.IsNotNull(chunk);
            Assert.AreEqual(chunkSize, chunk.Length);
            Assert.AreEqual(stream.Length, stream.Position);
            Assert.IsTrue(stream.CanRead);
        }

        [TestMethod]
        public void BuildSegmentIdWithValidSegmentNumber()
        {
            var key = "a/b/";
            var segment = 12345;
            var expectedKey = "a/b/0000012345";

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segmentKey = provider.BuildSegmentKey(key, segment);

            Assert.AreEqual(expectedKey, segmentKey);
        }

        [TestMethod]
        public void BuildSegmentIdWithValidSegmentNumberAndLeadingSlashInFolderName()
        {
            var key = "/a/b/";
            var segment = 12345;
            var expectedKey = "a/b/0000012345";

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var segmentKey = provider.BuildSegmentKey(key, segment);

            Assert.AreEqual(expectedKey, segmentKey);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildSegmentIdWithValidNegativeSegmentNumber()
        {
            var key = "key";
            var segment = -12345;

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.BuildSegmentKey(key, segment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildSegmentIdWithNullKey()
        {
            string key = null;
            var segment = 12345;

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.BuildSegmentKey(key, segment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildSegmentIdWithEmptyKey()
        {
            string key = string.Empty;
            var segment = 12345;

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.BuildSegmentKey(key, segment);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void BuildStorageManifestWithNullSegmentCollection()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.BuildStorageManifest(containerName, manifestname, metadata, null, segmentContainer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildStorageManifestWithEmptySegmentCollection()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            provider.BuildStorageManifest(containerName, manifestname, metadata, new List<StorageObject>(), segmentContainer);
        }

        [TestMethod]
        public void HasMinSizeSegmentsWithEmptySegmentCollection()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            Assert.IsFalse(provider.HasMinSizeSegments(new List<StorageObject>(), 1048576));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HasMinSizeSegmentsWithNullSegmentCollection()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

           Assert.IsFalse(provider.HasMinSizeSegments(null, 1048576));
        }

        [TestMethod]
        public void BuildStorageManifestWithSingleSegmentOfValidSize()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();
            var sObject = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 2048576, "text");
            var segments = new List<StorageObject>() { sObject };

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var manifest = provider.BuildStorageManifest(containerName, manifestname, metadata, segments, segmentContainer);

            Assert.IsInstanceOfType(manifest, typeof(StaticLargeObjectManifest));
        }

        [TestMethod]
        public void BuildStorageManifestWithSingleSegmentOfLessThenMinSize()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();
            var sObject = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 48576, "text");
            var segments = new List<StorageObject>() { sObject };

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var manifest = provider.BuildStorageManifest(containerName, manifestname, metadata, segments, segmentContainer);

            Assert.IsInstanceOfType(manifest, typeof(DynamicLargeObjectManifest));
            Assert.AreEqual(string.Format("{0}/{1}/", segmentContainer, manifestname), ((DynamicLargeObjectManifest)manifest).SegmentsPath);
        }

        [TestMethod]
        public void BuildStorageManifestWithMultipleSegmentOfValidSize()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();
            var sObject = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 2048576, "text");
            var sObject2 = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 2048576, "text");
            var segments = new List<StorageObject>() { sObject, sObject2 };

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var manifest = provider.BuildStorageManifest(containerName, manifestname, metadata, segments, segmentContainer);

            Assert.IsInstanceOfType(manifest, typeof(StaticLargeObjectManifest));
        }

        [TestMethod]
        public void BuildStorageManifestWithMultipleSegmentAndOneLessthenMinSize()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();
            var sObject = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 2048576, "text");
            var sObject2 = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 576, "text");
            var sObject3 = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 2048576, "text");
            var segments = new List<StorageObject>() { sObject, sObject2, sObject3 };

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var manifest = provider.BuildStorageManifest(containerName, manifestname, metadata, segments, segmentContainer);

            Assert.IsInstanceOfType(manifest, typeof(DynamicLargeObjectManifest));
        }

        [TestMethod]
        public void BuildStorageManifestWithMultipleSegmentOfValidSizeAndLastSegmentUnderMin()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();
            var sObject = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 2048576, "text");
            var sObject2 = new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 48576, "text");
            var segments = new List<StorageObject>() { sObject, sObject2 };

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var manifest = provider.BuildStorageManifest(containerName, manifestname, metadata, segments, segmentContainer);

            Assert.IsInstanceOfType(manifest, typeof(StaticLargeObjectManifest));
        }

        [TestMethod]
        public void BuildStorageManifestWithOverTheMaxNumberOfSegmentsForStatic()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();
            var segments = new List<StorageObject>();
            for (var i = 0; i < 1050; i++)
            {
                segments.Add( new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 2048576, "text"));
            }

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var manifest = provider.BuildStorageManifest(containerName, manifestname, metadata, segments, segmentContainer);

            Assert.IsInstanceOfType(manifest, typeof(DynamicLargeObjectManifest));
        }

        [TestMethod]
        public void BuildStorageManifestWithOverTheMaxSizeForStatic()
        {
            var segmentContainer = "segments";
            var containerName = "TestContainer";
            var manifestname = "TestManifest";
            var metadata = new Dictionary<string, string>();
            var segments = new List<StorageObject>();
            for (var i = 0; i < 999; i++)
            {
                segments.Add(new StorageObject("TestObject", containerName, DateTime.UtcNow, "12345", 82048576, "text"));
            }

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());

            var manifest = provider.BuildStorageManifest(containerName, manifestname, metadata, segments, segmentContainer);

            Assert.IsInstanceOfType(manifest, typeof(DynamicLargeObjectManifest));
        }

        [TestMethod]
        public async Task CanCreateStorageManifestWithStaticManifest()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var client = new TestStorageServiceClient();
            var staticManifestCreateWasCalled = false;

            var manifest = new StaticLargeObjectManifest(containerName, objectName, new Dictionary<string, string>(), new List<StorageObject>());

            client.CreateStaticStorageManifestDelegate = async (c, o, m, objs) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                staticManifestCreateWasCalled = true;
                return await Task.Run(() => manifest);
            };

            var provider = new LargeStorageObjectCreator(client);
            await provider.CreateStorageManifest(manifest);
            Assert.IsTrue(staticManifestCreateWasCalled);
        }

        [TestMethod]
        public async Task CanCreateStorageManifestWithDynamicManifest()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var segPath = "a/b/";
            var client = new TestStorageServiceClient();
            var dynamicManifestCreateWasCalled = false;

            var manifest = new DynamicLargeObjectManifest(containerName, objectName, new Dictionary<string, string>(), segPath);

            client.CreateDynamicStorageManifestDelegate = async (c, o, m, segs) =>
            {
                Assert.AreEqual(c, containerName);
                Assert.AreEqual(o, objectName);
                Assert.AreEqual(segs, segPath);
                dynamicManifestCreateWasCalled = true;
                return await Task.Run(() => manifest);
            };

            var provider = new LargeStorageObjectCreator(client);
            await provider.CreateStorageManifest(manifest);
            Assert.IsTrue(dynamicManifestCreateWasCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CanCreateStorageManifestWithNullManifest()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            await provider.CreateStorageManifest(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateStorageManifestWithUnsupportedManifest()
        {
            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            await provider.CreateStorageManifest(new TestStorageManifest("a", "a/b/c"));
        }

        [TestMethod]
        public async Task CanCreateLargeObjectWithStaticManifest()
        {
            var client = new TestStorageServiceClient();
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var segPath = "segs";
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();
            var currentSegment = 0;
            var resContent = new StringBuilder();

            var segmentsFolder = new StorageFolder(objectName, new List<StorageFolder>());
            var segmentsContainer = new StorageContainer(segmentContainer, new Dictionary<string, string>());
            segmentsContainer.Folders = new List<StorageFolder>() { segmentsFolder };

            var manifestAsObj = new StorageObject(containerName, objectName, DateTime.UtcNow, "12345", 2048576, "text");
            var manifest = new DynamicLargeObjectManifest(containerName, objectName, segPath);

            client.GetStorageContainerDelegate = async (c) =>
            {
                Assert.AreEqual(segmentContainer, c);
                return await Task.Run(() => segmentsContainer);
            };

            client.CreateStaticStorageManifestDelegate = async (c,o,m,objs) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                Assert.AreEqual(3, objs.Count());
                return await Task.Run(() => manifest);
            };

            client.GetStorageObjectDelegate = async (c, o) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                return await Task.Run(() => manifestAsObj);
            };

            client.GetStorageFolderDelegate = async (c, o) =>
            {
                Assert.AreEqual(segmentContainer, c);
                Assert.AreEqual(objectName, o);
                return await Task.Run(() => segmentsFolder);
            };

            client.CreateStorageObjectDelegate = async (c, o, m, s) =>
            {
                Assert.AreEqual(segmentContainer, c);
                Assert.AreEqual(objectName + "/" + currentSegment.ToString("D10"), o);
                var item = new StorageObject(c, o, DateTime.UtcNow, "12345", 2048576, "text", m);
                segmentsFolder.Objects.Add(item);
                currentSegment++;
                using (var sr = new StreamReader(s))
                {
                    resContent.Append(sr.ReadToEnd());
                }
                return await Task.Run(() => item);
            };

            var provider = new LargeStorageObjectCreator(client);
            var res = await provider.Create(containerName, objectName, metadata, contentStream, 3, segmentContainer);

            Assert.AreEqual(manifestAsObj, res);
            Assert.AreEqual(3, currentSegment);
            Assert.AreEqual(content, resContent.ToString());
        }

        [TestMethod]
        public async Task CanCreateLargeObjectWithDynamicManifest()
        {
            var client = new TestStorageServiceClient();
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var segPath = segmentContainer +"/" + objectName +"/";
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();
            var currentSegment = 0;
            var resContent = new StringBuilder();

            var segmentsFolder = new StorageFolder(objectName, new List<StorageFolder>());
            var segmentsContainer = new StorageContainer(segmentContainer, new Dictionary<string, string>());
            segmentsContainer.Folders = new List<StorageFolder>() { segmentsFolder };

            var manifestAsObj = new StorageObject(containerName, objectName, DateTime.UtcNow, "12345", 2048576, "text");
            var manifest = new DynamicLargeObjectManifest(containerName, objectName, segPath);

            client.GetStorageContainerDelegate = async (c) =>
            {
                Assert.AreEqual(segmentContainer, c);
                return await Task.Run(() => segmentsContainer);
            };

            client.CreateDynamicStorageManifestDelegate = async (c, o, m, segs) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                Assert.AreEqual(segPath, segs);
                return await Task.Run(() => manifest);
            };

            client.GetStorageObjectDelegate = async (c, o) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                return await Task.Run(() => manifestAsObj);
            };

            client.GetStorageFolderDelegate = async (c, o) =>
            {
                Assert.AreEqual(segmentContainer, c);
                Assert.AreEqual(objectName, o);
                return await Task.Run(() => segmentsFolder);
            };

            client.CreateStorageObjectDelegate = async (c, o, m, s) =>
            {
                Assert.AreEqual(segmentContainer, c);
                Assert.AreEqual(objectName + "/" + currentSegment.ToString("D10"), o);
                var item = new StorageObject(c, o, DateTime.UtcNow, "12345", 576, "text", m);
                segmentsFolder.Objects.Add(item);
                currentSegment++;
                using (var sr = new StreamReader(s))
                {
                    resContent.Append(sr.ReadToEnd());
                }
                return await Task.Run(() => item);
            };

            var provider = new LargeStorageObjectCreator(client);
            var res = await provider.Create(containerName, objectName, metadata, contentStream, 3, segmentContainer);

            Assert.AreEqual(manifestAsObj, res);
            Assert.AreEqual(3, currentSegment);
            Assert.AreEqual(content, resContent.ToString());
        }

        [TestMethod]
        public async Task CanCreateLargeObjectWithExistingSegments()
        {
            var client = new TestStorageServiceClient();
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var segPath = "segs";
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();
            
            var currentSegment = 1;
            var resContent = new StringBuilder().Append("THIS IS A LOT OF CO");

            var segmentsFolder = new StorageFolder(objectName, new List<StorageFolder>());
            segmentsFolder.Objects.Add(new StorageObject(objectName + "/" + 0.ToString("D10"), segmentContainer, DateTime.UtcNow, "1234", 2048576, "text"));
            var badSegment = new StorageObject(objectName + "/" + 1.ToString("D10"), segmentContainer, DateTime.UtcNow, "1234", 2048576, "text");
            segmentsFolder.Objects.Add(badSegment);
            
            var segmentsContainer = new StorageContainer(segmentContainer, new Dictionary<string, string>());
            segmentsContainer.Folders = new List<StorageFolder>() { segmentsFolder };

            var manifestAsObj = new StorageObject(containerName, objectName, DateTime.UtcNow, "12345", 2048576, "text");
            var manifest = new DynamicLargeObjectManifest(containerName, objectName, segPath);

            client.DeleteStorageObjectDelegate = async (c, o) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(c, segmentContainer);
                    Assert.AreEqual(objectName + "/" + 1.ToString("D10"), o);
                    segmentsFolder.Objects.Remove(badSegment);
                });
            };

            client.GetStorageContainerDelegate = async (c) =>
            {
                Assert.AreEqual(segmentContainer, c);
                return await Task.Run(() => segmentsContainer);
            };

            client.CreateStaticStorageManifestDelegate = async (c, o, m, objs) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                Assert.AreEqual(3, objs.Count());
                return await Task.Run(() => manifest);
            };

            client.GetStorageObjectDelegate = async (c, o) =>
            {
                Assert.AreEqual(containerName, c);
                Assert.AreEqual(objectName, o);
                return await Task.Run(() => manifestAsObj);
            };

            client.GetStorageFolderDelegate = async (c, o) =>
            {
                Assert.AreEqual(segmentContainer, c);
                Assert.AreEqual(objectName, o);
                return await Task.Run(() => segmentsFolder);
            };

            client.CreateStorageObjectDelegate = async (c, o, m, s) =>
            {
                Assert.AreEqual(segmentContainer, c);
                Assert.AreEqual(objectName + "/" + currentSegment.ToString("D10"), o);
                var item = new StorageObject(c, o, DateTime.UtcNow, "12345", 2048576, "text", m);
                segmentsFolder.Objects.Add(item);
                currentSegment++;
                using (var sr = new StreamReader(s))
                {
                    resContent.Append(sr.ReadToEnd());
                }
                return await Task.Run(() => item);
            };

            var provider = new LargeStorageObjectCreator(client);
            var res = await provider.Create(containerName, objectName, metadata, contentStream, 3, segmentContainer);

            Assert.AreEqual(manifestAsObj, res);
            Assert.AreEqual(3, currentSegment);
            Assert.AreEqual(content, resContent.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullContainerName()
        {
            var segmentContainer = "LargeObjectSegments";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(null, objectName, metadata, contentStream, 3, segmentContainer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithEmptyContainerName()
        {
            var segmentContainer = "LargeObjectSegments";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(string.Empty, objectName, metadata, contentStream, 3, segmentContainer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullObjectName()
        {
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(containerName, null, metadata, contentStream, 3, segmentContainer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithEmptyObjectName()
        {
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(containerName, string.Empty, metadata, contentStream, 3, segmentContainer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullMetadata()
        {
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(containerName, objectName, null, contentStream, 3, segmentContainer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullContent()
        {
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(containerName, objectName, metadata, null, 3, segmentContainer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CannotCreateLargeObjectWithNullSegmentContainerName()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(containerName, objectName, metadata, contentStream, 3, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithEmptySegmentContainerName()
        {
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(containerName, objectName, metadata, contentStream, 3, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithNegativeSegmentCount()
        {
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(containerName, objectName, metadata, contentStream, -3, segmentContainer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CannotCreateLargeObjectWithZeroSegmentCount()
        {
            var segmentContainer = "LargeObjectSegments";
            var containerName = "TestContainer";
            var objectName = "TestObject";
            var metadata = new Dictionary<string, string>();
            var content = "THIS IS A LOT OF CONTENT THAT WILL AND CAN BE CHOPPED UP";
            var contentStream = content.ConvertToStream();

            var provider = new LargeStorageObjectCreator(new TestStorageServiceClient());
            var res = await provider.Create(containerName, objectName, metadata, contentStream, 0, segmentContainer);
        }

        [TestMethod]
        public async Task CanGetSegmentsFolderWithContainerAndFolder()
        {
            var client = new TestStorageServiceClient();
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var container = new StorageContainer(containerName, new Dictionary<string, string>());
            var folder = new StorageFolder(objectName, new List<StorageFolder>());
            container.Folders = new List<StorageFolder>() { folder };

            client.GetStorageContainerDelegate = async (c) =>
            {
                Assert.AreEqual(c, container.Name);
                return await Task.Run(() => container);
            };

            var provider = new LargeStorageObjectCreator(client);
            var res = await provider.GetSegmentsFolder(containerName, objectName);

            Assert.AreEqual(folder, res);
        }

        [TestMethod]
        public async Task CanGetSegmentsFolderWithContainerAndNoFolder()
        {
            var client = new TestStorageServiceClient();
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var container = new StorageContainer(containerName, new Dictionary<string, string>());
            var folder = new StorageFolder(objectName, new List<StorageFolder>());
            container.Folders = new List<StorageFolder>() { folder };

            client.GetStorageContainerDelegate = async (c) =>
            {
                Assert.AreEqual(container.Name, c);
                return await Task.Run(() => container);
            };

            client.CreateStorageFolderDelegate = async (c, f) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(container.Name, c);
                    Assert.AreEqual(folder.FullName, f);
                });
            };

            client.GetStorageFolderDelegate = async (c, f) =>
            {
                Assert.AreEqual(container.Name, c);
                Assert.AreEqual(folder.FullName, f);
                return await Task.Run(() => folder);
            };

            var provider = new LargeStorageObjectCreator(client);
            var res = await provider.GetSegmentsFolder(containerName, objectName);

            Assert.AreEqual(folder, res);
        }

        [TestMethod]
        public async Task CanGetSegmentsFolderWithNoContainer()
        {
            var client = new TestStorageServiceClient();
            var containerName = "TestContainer";
            var objectName = "TestObject";

            var container = new StorageContainer(containerName, new Dictionary<string, string>());
            var folder = new StorageFolder(objectName, new List<StorageFolder>());
            container.Folders = new List<StorageFolder>() { folder };

            client.GetStorageContainerDelegate = (c) =>
            {
                throw new InvalidOperationException(string.Format("Failed to get storage container '{0}'. The remote server returned the following status code: '{1}'.", containerName, HttpStatusCode.NotFound));
            };

            client.CreateStorageContainerDelegate = async (c, m) =>
            {
                await Task.Run(() => Assert.AreEqual(container.Name, c));
            };

            client.CreateStorageFolderDelegate = async (c, f) =>
            {
                await Task.Run(() =>
                {
                    Assert.AreEqual(container.Name, c);
                    Assert.AreEqual(folder.FullName, f);
                });
            };

            client.GetStorageFolderDelegate = async (c, f) =>
            {
                Assert.AreEqual(container.Name, c);
                Assert.AreEqual(folder.FullName, f);
                return await Task.Run(() => folder);
            };

            var provider = new LargeStorageObjectCreator(client);
            var res = await provider.GetSegmentsFolder(containerName, objectName);

            Assert.AreEqual(folder, res);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CannotGetSegmentsFolderWhenGetContainerFailsWithNon404()
        {
            var client = new TestStorageServiceClient();
            var containerName = "TestContainer";
            var objectName = "TestObject";

            client.GetStorageContainerDelegate = (c) =>
            {
                throw new InvalidOperationException(string.Format("Failed to get storage container '{0}'. The remote server returned the following status code: '500'.", containerName));
            };

            var provider = new LargeStorageObjectCreator(client);
            await provider.GetSegmentsFolder(containerName, objectName);
        }
    }
}
