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
using System.Threading.Tasks;
using OpenStack.Common;

namespace OpenStack.Storage
{
    /// <inheritdoc/>
    internal class LargeStorageObjectCreator : ILargeStorageObjectCreator
    {
        internal IStorageServiceClient StorageClient;

        /// <summary>
        /// Creates a new instance of the LargeStorageObjectCreator class.
        /// </summary>
        /// <param name="storageClient">The storage service client to use when creating the large object.</param>
        internal LargeStorageObjectCreator(IStorageServiceClient storageClient)
        {
            storageClient.AssertIsNotNull("storageClient","Cannot create a large object with a null storage client.");
            this.StorageClient = storageClient;
        }

        /// <inheritdoc/>
        public async Task<StorageObject> Create(string containerName, string objectName, IDictionary<string, string> metadata, Stream content, int numberOfSegments, string segmentsContainer)
        {
            containerName.AssertIsNotNullOrEmpty("containerName", "Cannot create a large storage object with a null or empty container name.");
            objectName.AssertIsNotNullOrEmpty("objectName", "Cannot create a large storage object with a null or empty name.");
            metadata.AssertIsNotNull("metadata", "Cannot create a large storage object with null metadata.");
            content.AssertIsNotNull("content", "Cannot create a large storage object with null content.");
            segmentsContainer.AssertIsNotNullOrEmpty("segmentsContainer","Cannot  create a large object with a null or empty segments container name.");

            if (numberOfSegments <= 0)
            {
                throw new ArgumentException("Cannot create a large object with zero or less segments.", "numberOfSegments");
            }
            
            var segmentFolder = await this.GetSegmentsFolder(segmentsContainer, objectName);

            var chunkSize = this.GetChunkSize(content.Length, numberOfSegments);
            var lastSegmentAndId = this.GetLastSegmentIdAndName(segmentFolder);
            var lastSegmentId = lastSegmentAndId.Key;
            var lastSegmentFullName = string.Format("{0}/{1}", objectName, lastSegmentAndId.Value);
            var offset = lastSegmentId * chunkSize;
            
            if (lastSegmentId > 0)
            {
                //we should drop the last segment (as it's very possible it was corrupted)
                await StorageClient.DeleteStorageObject(segmentsContainer, lastSegmentFullName);
            }

            //Seek to the correct location based on whatever segments have been uploaded, if any
            content.Seek(offset, SeekOrigin.Begin);

            while (content.Position < content.Length)
            {
                //If the remaining data in the stream is less then the chunk size, reset the chunk size to the remaining data size.
                if ((content.Length - content.Position) < chunkSize)
                {
                    chunkSize = (content.Length - content.Position);
                }

                await CreateSegment(content, chunkSize, objectName, lastSegmentId, metadata, segmentsContainer);
                lastSegmentId++;
            }

            var segments = await StorageClient.GetStorageFolder(segmentsContainer, objectName);

            return await CreateLargeObjectManifest(containerName, objectName, metadata, segments.Objects, segmentsContainer);
        }

        /// <summary>
        /// Creates a large objects manifest.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="manifestName">The name of the manifest.</param>
        /// <param name="metadata">The associated metadata for the manifest.</param>
        /// <param name="segments">The collection of segments that represent the large object.</param>
        /// <param name="segmentContainerName">The container that contains the object segments.</param>
        /// <returns>The StorageObject representation of the final manifest.</returns>
        internal async Task<StorageObject> CreateLargeObjectManifest(string containerName, string manifestName, IDictionary<string, string> metadata, ICollection<StorageObject> segments, string segmentContainerName)
        {
            try
            {
                var manifest = this.BuildStorageManifest(containerName, manifestName, metadata, segments, segmentContainerName);
                await this.CreateStorageManifest(manifest);
                return await StorageClient.GetStorageObject(containerName, manifestName);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Format("Could not create large object '{0}/{1}'. Could not create object manifest. See inner exception for details.", containerName, manifestName), ex);
            }
        }

        /// <summary>
        /// Creates a storage manifest on the remote OpenStack instance.
        /// </summary>
        /// <param name="manifest">The manifest to create.</param>
        /// <returns>An async task.</returns>
        internal async Task CreateStorageManifest(StorageManifest manifest)
        {
            manifest.AssertIsNotNull("manifest", "Cannot create a manifest that is null.");

            var dynoManifest = manifest as DynamicLargeObjectManifest;
            var staticManifest = manifest as StaticLargeObjectManifest;
            if (dynoManifest == null && staticManifest == null)
            {
                throw new ArgumentException(string.Format("Cannot create manifest. The given manifest type '{0}' is not supported.", manifest.GetType().Name), "manifest");
            }

            if (dynoManifest != null)
            {
                await this.StorageClient.CreateStorageManifest(dynoManifest.ContainerName, dynoManifest.FullName, dynoManifest.Metadata, dynoManifest.SegmentsPath);
            }

            if (staticManifest != null)
            {
                await this.StorageClient.CreateStorageManifest(staticManifest.ContainerName, staticManifest.FullName, staticManifest.Metadata, staticManifest.Objects);
            }
        }

        /// <summary>
        /// Creates a segment of a large object.
        /// </summary>
        /// <param name="content">The stream that holds the large objects content.</param>
        /// <param name="chunkSize">The size of the segment.</param>
        /// <param name="objectName">The name of the large object.</param>
        /// <param name="segmentId">The Id for the segment.</param>
        /// <param name="metadata">The associated metadata for the large object.</param>
        /// <param name="segmentsContainer">The name of the container that will hold the object segments.</param>
        /// <returns>The StorageObject representation of the segment.</returns>
        internal async Task<StorageObject> CreateSegment(Stream content, long chunkSize, string objectName, int segmentId, IDictionary<string, string> metadata, string segmentsContainer)
        {
            try
            {
                var chunk = this.GetObjectChunk(content, chunkSize);
                var segmentKey = this.BuildSegmentKey(objectName, segmentId);
                return await this.StorageClient.CreateStorageObject(segmentsContainer, segmentKey, metadata, chunk);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Format("Could not create large object '{0}'. Could not create object segment '{1}'. See inner exception for details.", objectName, segmentId), ex);
            }
        }

        /// <summary>
        /// Gets the folder that should be used for storing segments of large objects.
        /// </summary>
        /// <param name="containerName">The name of the parent container.</param>
        /// <param name="objectName">The name of the large object.</param>
        /// <returns>A StorageFolder object.</returns>
        internal async Task<StorageFolder> GetSegmentsFolder(string containerName, string objectName)
        {
            StorageContainer container = null;
            try
            {
                container = await this.StorageClient.GetStorageContainer(containerName);
            }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.Contains(HttpStatusCode.NotFound.ToString()))
                {
                    throw;
                }
            }

            if (container == null)
            {
                await this.StorageClient.CreateStorageContainer(containerName, new Dictionary<string, string>());
                await this.StorageClient.CreateStorageFolder(containerName, objectName);
                return await this.StorageClient.GetStorageFolder(containerName, objectName);
            }

            var segmentFolder =
                container.Folders.FirstOrDefault(
                    f => string.Equals(f.FullName, objectName, StringComparison.Ordinal));

            if (segmentFolder == null)
            {
                await this.StorageClient.CreateStorageFolder(containerName, objectName);
                return await this.StorageClient.GetStorageFolder(containerName, objectName);
            }

            return segmentFolder;
        }


        /// <summary>
        /// Calculates the optimal size for each chunk of an object that needs to be segmented.
        /// </summary>
        /// <param name="objectSize">The total size of the object.</param>
        /// <param name="numberOfSegments">The desired number of segments.</param>
        /// <returns>The optimal chunk size, in bytes.</returns>
        internal long GetChunkSize(long objectSize, int numberOfSegments)
        {
            if (objectSize <= 0)
            {
                throw new ArgumentException("Chunk size cannot be calculated. object size " + objectSize + " is less then or equal to zero.", "objectSize");
            }

            if (numberOfSegments < 0)
            {
                throw new ArgumentException("Chunk size cannot be calculated. Number of segments must be greater then or equal to zero.", "numberOfSegments");
            }

            return numberOfSegments == 0 ? objectSize : Convert.ToInt64(Math.Ceiling((double)objectSize / (double)numberOfSegments));
        }

        /// <summary>
        /// Gets the Id of the segment that the given storage object represents.
        /// </summary>
        /// <param name="storageObject">The storage object to extract the Id from.</param>
        /// <returns>The Id of the segment that the file represents.</returns>
        internal int GetSegmentIdFromKey(StorageObject storageObject)
        {
            try
            {
                var segmentId = Convert.ToInt32(storageObject.Name);
                if (segmentId < 0)
                {
                    throw new System.FormatException("Cannot get segment Id from key. Segment number cannot be negative.");
                }
                return segmentId;

            }
            catch (System.OverflowException)
            {
                //if the segment number is bigger than an int32, then it's an invalid format
                throw new System.FormatException("Segment number is too large. The segment number must be a valid Int32.");
            }
            catch (System.FormatException)
            {
                //if the segment number is bigger than an int32, then it's an invalid format
                throw new System.FormatException(string.Format("The segment's file name does not have the correct format. '{0}' is invalid.", storageObject.FullName));
            }
        }

        /// <summary>
        /// Gets the last segment of a large object that was found in the given folder.
        /// </summary>
        /// <param name="segmentFolder">The folder that contains the segments of the large object.</param>
        /// <returns>A key value pair that represents the segment Id and the name of the storage object that represents that segment.</returns>
        internal KeyValuePair<int, string> GetLastSegmentIdAndName(StorageFolder segmentFolder)
        {
            segmentFolder.AssertIsNotNull("segmentFolder","Cannot get segment id or name given a null segment folder.");
            if (segmentFolder.Objects.Count == 0)
            {
                return new KeyValuePair<int, string>(0, string.Empty);
            }

            try
            {
                var segments = segmentFolder.Objects.Select(o => new KeyValuePair<int, string>(GetSegmentIdFromKey(o), o.Name)).ToList();
                segments.Sort((kvp1, kvp2) => kvp1.Key.CompareTo(kvp2.Key));

                return segments.Last();
            }
            catch (System.FormatException)
            {
                //If the segment id in the file is malformed (IE does not conform to the format we expect)
                //Then we cannot get the last segment.
                return new KeyValuePair<int, string>(0, string.Empty);
            }
        }

        /// <summary>
        /// Gets a portion of a stream that represents a large object.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="chunkSize">The size of the chunk to return.</param>
        /// <returns>A stream that represents the chunk of the large object.</returns>
        internal Stream GetObjectChunk(Stream input, long chunkSize)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (!input.CanRead)
            {
                throw new InvalidOperationException("Cannot read from input stream.");
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentException("Cannot get chunk. Chunk size must be greater then zero.", "chunkSize");
            }

            var chunkBytesRead = 0;
            var buffer = new byte[chunkSize];

            while (chunkBytesRead < chunkSize)
            {
                var bytesRead = input.Read(buffer, chunkBytesRead, Convert.ToInt32(chunkSize - chunkBytesRead));
                if (bytesRead == 0)
                {
                    break;
                }
                chunkBytesRead += bytesRead;
            }

            return new MemoryStream(buffer);
        }

        /// <summary>
        /// Builds the full name for a segment of a large object.
        /// </summary>
        /// <param name="folderName">The full name of the folder that will contain this segment.</param>
        /// <param name="segmentNumber">The segment number of this segment.</param>
        /// <returns>The full name for the segment.</returns>
        internal string BuildSegmentKey(string folderName, int segmentNumber)
        {
            folderName.AssertIsNotNullOrEmpty("folderName","Cannot build segment key with a null or empty folder name.");
            if (segmentNumber < 0)
            {
                throw new ArgumentException("Cannot build segment Id. Segment number must be greater than zero.", "segmentNumber");
            }

            return string.Format("{0}/{1}", folderName.Trim('/'), segmentNumber.ToString("D10"));
        }

        /// <summary>
        /// Builds a StorageManifest for a large object. 
        /// The type of the manifest, static or dynamic, will be inferred based on the segments collection.
        /// </summary>
        /// <param name="containerName">The parent container name.</param>
        /// <param name="manifestName">The name of the manifest.</param>
        /// <param name="metadata">The associated metadata for the manifest.</param>
        /// <param name="segments">A collection of StorageObjects that represent the segments of the large object.</param>
        /// <param name="segmentContainer">A name of the container that contains the object segments.</param>
        /// <returns>A StorageManifest object.</returns>
        internal StorageManifest BuildStorageManifest(string containerName, string manifestName, IDictionary<string, string> metadata, ICollection<StorageObject> segments, string segmentContainer)
        {
            segments.AssertIsNotNull("segments", "Cannot build a manifest with a null segment collection.");
            if (segments.Count == 0)
            {
                throw new ArgumentException("Cannot build a manifest with an empty segment collection.", "segments");
            }

            //Based on the OpenStack Swift API 1.0 documentation:
            //1. All segments in a static large object must be at least 1MB in size (except for the last segment)
            //2. There is a max of 1000 segments.
            //3. The largest static file that can be represented by a manifest and still copied is 5GB
            var staticSegmentLimit = 1000;
            var minStaticSegmentSize = 1048576;
            var maxStaticObjectSize = 5368709120;

            var totalSize = segments.Select(s => s.Length).Sum();
            if (segments.Count() < staticSegmentLimit && HasMinSizeSegments(segments, minStaticSegmentSize) && totalSize < maxStaticObjectSize)
            {
                //If we can represent the segments using a static manifest, we should.
                return new StaticLargeObjectManifest(containerName, manifestName, segments);
            }

            return new DynamicLargeObjectManifest(containerName, manifestName, string.Format("{0}/{1}/", segmentContainer, manifestName));
        }

        /// <summary>
        /// Determines if the collection of StorageObjects meets the minimum size requirements for a static large object manifest.
        /// </summary>
        /// <param name="segments">A collection of StorageObjects.</param>
        /// <param name="minSegmentSize">The minimum size in bytes for segments.</param>
        /// <returns>A value indicating if the collection meets the minimum size criteria.</returns>
        internal bool HasMinSizeSegments(ICollection<StorageObject> segments, long minSegmentSize)
        {
            segments.AssertIsNotNull("segments", "Cannot ensure that a collection of segments meets minimum size criteria with a null segment collection.");
            var segmentsToCheck = new List<StorageObject>();
            if (segments.Count == 0)
            {
                //If the collection is empty it does not have any segments that are of the min size.
                return false;
            }

            if (segments.Count == 1)
            {
                //If the collection only has one element then it must meet the criteria (even though the spec says the last segment can be any size).
                segmentsToCheck.Add(segments.First());
            }
            else
            {
                //All segments except the last one must be large then the min size.
                segmentsToCheck.AddRange(segments.Take(segments.Count -1));
            }

            return segmentsToCheck.All(s => s.Length > minSegmentSize);
        }
    }
}
