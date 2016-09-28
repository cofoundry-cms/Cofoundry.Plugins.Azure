using Cofoundry.Domain.Data;
using Conditions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private static ConcurrentDictionary<string, object> _initializedContainers = new ConcurrentDictionary<string, object>();

        public AzureBlobFileService(
            AzureBlobFileServiceSettings settings
            )
        {
            Condition.Requires(settings).IsNotNull();

            var storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);
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
        public bool Exists(string containerName, string fileName)
        {
            var container = GetBlobContainer(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);
            return blockBlob.Exists();
        }

        /// <summary>
        /// Gets the specified file as a Stream. 
        /// </summary>
        /// <param name="containerName">The name of the container to look for the file</param>
        /// <param name="fileName">The name of the file to get</param>
        /// <returns>Stream reference to the file.</returns>
        public Stream Get(string containerName, string fileName)
        {
            var container = GetBlobContainer(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            Stream stream = null;

            try
            {
                return blockBlob.OpenRead();
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
        public void Create(string containerName, string fileName, Stream stream)
        {
            Create(containerName, fileName, stream, true);
        }

        /// <summary>
        /// Saves a file, creating a new file or overwriting a file if it already exists.
        /// </summary>
        public void CreateOrReplace(string containerName, string fileName, Stream stream)
        {
            var container = GetBlobContainer(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            if (stream.Position != 0)
            {
                stream.Position = 0;
            }
            blockBlob.UploadFromStream(stream);
        }

        /// <summary>
        /// Creates a new file if it doesn't exist already, otherwise the existing file is left in place.
        /// </summary>
        public void CreateIfNotExists(string containerName, string fileName, Stream stream)
        {
            Create(containerName, fileName, stream, false);
        }

        /// <summary>
        /// Deletes a file from the container if it exists.
        /// </summary>
        /// <param name="containerName">The name of the container containing the file to delete</param>
        /// <param name="fileName">Name of the file to delete</param>
        public void Delete(string containerName, string fileName)
        {
            var container = GetBlobContainer(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            blockBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }

        /// <summary>
        /// Deletes a directory including all files and sub-directories
        /// </summary>
        /// <param name="containerName">The name of the container containing the directory to delete</param>
        /// <param name="directoryName">The name of the directory to delete</param>
        public void DeleteDirectory(string containerName, string directoryName)
        {
            var container = GetBlobContainer(containerName);
            var directory = container.GetDirectoryReference(directoryName);
            DeleteBlobs(directory.ListBlobs(true));

        }

        /// <summary>
        /// Clears a directory deleting all files and sub-directories but not the directory itself
        /// </summary>
        /// <param name="containerName">The name of the container containing the directory to delete</param>
        /// <param name="directoryName">The name of the directory to delete</param>
        public void ClearDirectory(string containerName, string directoryName)
        {
            DeleteDirectory(containerName, directoryName);
        }

        /// <summary>
        /// Deletes all files in the container
        /// </summary>
        /// <param name="containerName">Name of the container to clear.</param>
        public void ClearContainer(string containerName)
        {
            var container = GetBlobContainer(containerName);
            DeleteBlobs(container.ListBlobs(null, true));
        }

        #endregion

        #region privates

        private void DeleteBlobs(IEnumerable<IListBlobItem> blobs)
        {
            foreach (var blobItem in blobs)
            {
                var blockBlob = blobItem as CloudBlockBlob;
                if (blockBlob != null)
                {
                    blockBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
                }
                else
                {
                    var pageBlob = blobItem as CloudPageBlob;
                    if (pageBlob != null)
                    {
                        pageBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
                    }
                }
            }
        }

        private void Create(string containerName, string fileName, Stream stream, bool throwExceptionIfNotExists)
        {
            var container = GetBlobContainer(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            try
            {
                // http://stackoverflow.com/a/14938608/716689

                if (stream.Position != 0)
                {
                    stream.Position = 0;
                }
                blockBlob.UploadFromStream(stream, accessCondition: AccessCondition.GenerateIfNoneMatchCondition("*"));
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

        private CloudBlobContainer GetBlobContainer(string containerName)
        {
            containerName = containerName.ToLower();
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);

            // initalize container
            if (_initializedContainers.TryAdd(containerName, null))
            {
                container.CreateIfNotExists();
            }

            return container;
        }

        #endregion
    }
}
