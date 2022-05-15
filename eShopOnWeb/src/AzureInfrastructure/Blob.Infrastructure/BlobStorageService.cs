using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Newtonsoft.Json;

namespace Blob.Infrastructure;
public class BlobStorageService
{
    private readonly AzureStorageConfig _azureStorageConfig;

    public BlobStorageService(AzureStorageConfig storageConfig)
    {
        _azureStorageConfig = storageConfig;
    }

    public async Task Initialize()
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(_azureStorageConfig.ConnectionString);
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(_azureStorageConfig.FileContainerName);
        await blobContainerClient.CreateIfNotExistsAsync();
    }

    public async Task Save(Stream stream, string name)
    {

        BlobClientOptions blobClientOptions = new BlobClientOptions();
        blobClientOptions.Retry.MaxRetries = 3;

        BlobServiceClient blobServiceClient = new BlobServiceClient(_azureStorageConfig.ConnectionString, blobClientOptions);

        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(_azureStorageConfig.FileContainerName);

        BlobClient blobClient = blobContainerClient.GetBlobClient(name);

        await blobClient.UploadAsync(stream);
    }
}
