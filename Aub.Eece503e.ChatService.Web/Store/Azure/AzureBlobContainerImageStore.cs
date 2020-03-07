using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class AzureBlobContainerImageStore : IImageStore
    {
        private readonly CloudBlobContainer _blobContainer;

        public AzureBlobContainerImageStore(IOptions<AzureStorageSettings> options)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(options.Value.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference(options.Value.BlobContainerName);
        }

        public async Task<string> Upload(byte[] imageData)
        {
            string str = Guid.NewGuid().ToString();
            bool blobExists = await _blobContainer.GetBlockBlobReference(str).ExistsAsync();
            if (blobExists)
            {
                throw new BlobAlreadyExistsException($"Blob with same name already exists");
            }

            try
            {
                CloudBlockBlob cloudBlockBlob = _blobContainer.GetBlockBlobReference(str);
                await cloudBlockBlob.UploadFromStreamAsync(new MemoryStream(imageData));
                return str;
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409) // conflict
                {
                    throw new BlobAlreadyExistsException($"Image {str} already exists");
                }
                throw new StorageErrorException($"Could not create image in storage, imageId = {str}", e);
            }
           
        }
        public async Task<byte[]> Download(string imageId)
        {

            bool blobExists = await _blobContainer.GetBlockBlobReference(imageId).ExistsAsync();
            if (!blobExists)
            {
                throw new BlobNotFoundException($"Image does not exists");
            }

            try
            {
                CloudBlockBlob cloudBlockBlob = _blobContainer.GetBlockBlobReference(imageId);

                byte[] downloadedImage;
                using (var stream = new MemoryStream())
                {
                    await cloudBlockBlob.DownloadToStreamAsync(stream);
                    downloadedImage = stream.ToArray();
                }
                return downloadedImage;
            }
            catch (StorageException e)
            {
                throw new StorageErrorException($"Could not find image in storage, imageId = {imageId}", e);
            }
           
        }
        public async Task Delete(string imageId)
        {
            bool blobExists = await _blobContainer.GetBlockBlobReference(imageId).ExistsAsync();
            if (!blobExists)
            {
                throw new BlobNotFoundException($"Image does not exists");
            }

            try
            {
                CloudBlockBlob cloudBlockBlob = _blobContainer.GetBlockBlobReference(imageId);
                await cloudBlockBlob.DeleteIfExistsAsync();
            }
            catch (StorageException e)
            {
                throw new StorageErrorException($"Could not delete image in storage, imageId = {imageId}", e);
            }
        }
    }
}
