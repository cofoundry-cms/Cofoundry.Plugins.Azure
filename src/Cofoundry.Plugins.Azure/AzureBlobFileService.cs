using Cofoundry.Core.Configuration;
using Cofoundry.Domain.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cofoundry.Plugins.Azure
{
    /// <summary>
    /// File system abstraction for Azure blob storage
    /// </summary>
    public class AzureBlobFileService : IFileStoreService
    {
        #region constructor 

        private readonly CloudBlobClient _blobClient;
        private static ConcurrentDictionary<string, byte> _initializedContainers = new ConcurrentDictionary<string, byte>();
        private static ConcurrentBag<string> _initializedContainers2 = new ConcurrentBag<string>();

        public AzureBlobFileService(
            AzureSettings settings
            )
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(settings.BlobStorageConnectionString))
            {
                throw new InvalidConfigurationException(typeof(AzureSettings), "The BlobStorageConnectionString is required to use the AzureBlobFileService");
            }

            var storageAccount = CloudStorageAccount.Parse(settings.BlobStorageConnectionString);
            _blobClient = storageAccount.CreateCloudBlobClient();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Determins if the specified file exists in the container
        /// </summary>
        /// <param name="containerName">The name of the container to look for the filew</param>
        /// <param name="fileName">Name of the file to look for</param>
        /// <returns>True if the file exists; otherwise false</returns>
        public async Task<bool> ExistsAsync(string containerName, string fileName)
        {
            var container = await GetBlobContainerAsync(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            return await blockBlob.ExistsAsync();
        }

        /// <summary>
        /// Gets the specified file as a Stream. 
        /// </summary>
        /// <param name="containerName">The name of the container to look for the file</param>
        /// <param name="fileName">The name of the file to get</param>
        /// <returns>Stream reference to the file.</returns>
        public async Task<Stream> GetAsync(string containerName, string fileName)
        {
            var container = await GetBlobContainerAsync(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            Stream stream = null;

            try
            {
                return await blockBlob.OpenReadAsync();
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }
            }

            return stream;
        }

        /// <summary>
        /// Creates a new file, throwing an exception if a file already exists with the same filename
        /// </summary>
        public Task CreateAsync(string containerName, string fileName, Stream stream)
        {
            return CreateAsync(containerName, fileName, stream, true);
        }

        /// <summary>
        /// Saves a file, creating a new file or overwriting a file if it already exists.
        /// </summary>
        public  async Task CreateOrReplaceAsync(string containerName, string fileName, Stream stream)
        {
            var container = await GetBlobContainerAsync(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            if (stream.Position != 0)
            {
                stream.Position = 0;
            }
            await blockBlob.UploadFromStreamAsync(stream);
        }

        /// <summary>
        /// Creates a new file if it doesn't exist already, otherwise the existing file is left in place.
        /// </summary>
        public Task CreateIfNotExistsAsync(string containerName, string fileName, Stream stream)
        {
            return CreateAsync(containerName, fileName, stream, false);
        }

        /// <summary>
        /// Deletes a file from the container if it exists.
        /// </summary>
        /// <param name="containerName">The name of the container containing the file to delete</param>
        /// <param name="fileName">Name of the file to delete</param>
        public async Task DeleteAsync(string containerName, string fileName)
        {
            var container = await GetBlobContainerAsync(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            await blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);
        }

        /// <summary>
        /// Deletes a directory including all files and sub-directories
        /// </summary>
        /// <param name="containerName">The name of the container containing the directory to delete</param>
        /// <param name="directoryName">The name of the directory to delete</param>
        public async Task DeleteDirectoryAsync(string containerName, string directoryName)
        {
            var container = await GetBlobContainerAsync(containerName);
            var directory = container.GetDirectoryReference(directoryName);

            BlobContinuationToken continuationToken = null;
            var blobs = new List<IListBlobItem>();

            do
            {
                // each segment is max 5000 items
                var segment = await directory.ListBlobsSegmentedAsync(true, BlobListingDetails.None, null, continuationToken, null, null);
                continuationToken = segment.ContinuationToken;
                blobs.AddRange(segment.Results);

            }
            while (continuationToken != null);

            await DeleteBlobsAsync(blobs);
        }

        /// <summary>
        /// Clears a directory deleting all files and sub-directories but not the directory itself
        /// </summary>
        /// <param name="containerName">The name of the container containing the directory to delete</param>
        /// <param name="directoryName">The name of the directory to delete</param>
        public Task ClearDirectoryAsync(string containerName, string directoryName)
        {
            return DeleteDirectoryAsync(containerName, directoryName);
        }

        /// <summary>
        /// Deletes all files in the container
        /// </summary>
        /// <param name="containerName">Name of the container to clear.</param>
        public async Task ClearContainerAsync(string containerName)
        {
            var container = await GetBlobContainerAsync(containerName);

            BlobContinuationToken continuationToken = null;
            var blobs = new List<IListBlobItem>();

            do
            {
                // each segment is max 5000 items
                var segment = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.None, null, continuationToken, null, null);
                continuationToken = segment.ContinuationToken;
                blobs.AddRange(segment.Results);

            }
            while (continuationToken != null);

            await DeleteBlobsAsync(blobs);
        }

        #endregion

        #region privates

        private async Task DeleteBlobsAsync(IEnumerable<IListBlobItem> blobs)
        {
            foreach (var blobItem in blobs)
            {
                var blockBlob = blobItem as CloudBlockBlob;
                if (blockBlob != null)
                {
                    await blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);
                }
                else
                {
                    var pageBlob = blobItem as CloudPageBlob;
                    if (pageBlob != null)
                    {
                        await pageBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);
                    }
                }
            }
        }

        private async Task CreateAsync(string containerName, string fileName, Stream stream, bool throwExceptionIfNotExists)
        {
            var container = await GetBlobContainerAsync(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            // Don't overwrite:
            // http://stackoverflow.com/a/14938608/716689
            var accessCondition = AccessCondition.GenerateIfNotExistsCondition();

            try
            {
                if (stream.Position != 0)
                {
                    stream.Position = 0;
                }

                await blockBlob.UploadFromStreamAsync(stream, accessCondition, null, null);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 409)
                {
                    if (throwExceptionIfNotExists) throw new InvalidOperationException("File already exists", ex);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<CloudBlobContainer> GetBlobContainerAsync(string containerName)
        {
            containerName = containerName.ToLower();
            var container = _blobClient.GetContainerReference(containerName);

            // initalize container
            if (_initializedContainers.TryAdd(containerName, 0))
            {
                await container.CreateIfNotExistsAsync();
            }

            return container;
        }

        #endregion
    }
}
