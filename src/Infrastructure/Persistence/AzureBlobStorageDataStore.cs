using Azure.Storage.Blobs;
using MyHealthSolution.Service.Application.Common.Exceptions;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MyHealthSolution.Service.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MyHealthSolution.Service.Infrastructure.Persistence
{


    public class AzureBlobStorageDataStore : IBlobDataStore
    {
        private readonly BlobStoreConfig storeConfiguration;

        public AzureBlobStorageDataStore(IOptionsMonitor<BlobStoreConfig> options)

        {
            this.storeConfiguration = options.CurrentValue;
        }

        public async Task<Stream> DownloadFileAsync(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new System.ArgumentException($"'{nameof(folder)}' cannot be null or empty", nameof(folder));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new System.ArgumentException($"'{nameof(fileName)}' cannot be null or empty", nameof(fileName));
            }
            // Create a BlobServiceClient object which will be used to create a container client
            var blobServiceClient = new BlobServiceClient(storeConfiguration.CorrespondenceBlobStorageConnectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(folder.ToLowerInvariant());
            if (!await containerClient.ExistsAsync())
            {
                // TODO - raise exception (determine if this is the right exception type)
                throw new DirectoryNotFoundException($"Folder {folder} does not exist. Please check the name.");
            }

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName.ToLowerInvariant());

            //Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);
            var ms = new MemoryStream();

            var fileExists = await blobClient.ExistsAsync();
            if(!fileExists)
            {
                // Specified file does not exist in blob storage.
                throw new FileNotFoundException(fileName);
            }

            await blobClient.DownloadToAsync(ms);
            ms.Position = 0;
            return ms;
        }

        public async Task UploadFileAsync(string folder, string fileName, Stream data)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new System.ArgumentException($"'{nameof(folder)}' cannot be null or empty", nameof(folder));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new System.ArgumentException($"'{nameof(fileName)}' cannot be null or empty", nameof(fileName));
            }

            if (data is null)
            {
                throw new System.ArgumentNullException(nameof(data));
            }
            // Create a BlobServiceClient object which will be used to create a container client
            var blobServiceClient = new BlobServiceClient(storeConfiguration.CorrespondenceBlobStorageConnectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(folder.ToLowerInvariant());
            await containerClient.CreateIfNotExistsAsync();

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName.ToLowerInvariant());

            // reset stream position
            data.Position = 0;
            //data.Position=0;
            await blobClient.UploadAsync(data, true);
        }

    }

}
