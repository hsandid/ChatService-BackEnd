using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class AzureTableImagesStore : IImageStore
    {
        private readonly CloudBlobContainer _blob;

        public AzureTableImagesStore(IOptions<AzureStorageSettings> options)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(options.Value.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            _blob = blobClient.GetContainerReference(options.Value.ImagesTableName);
        }

        public async Task Delete(string imageID)
        {
            string blobName = imageID;
            CloudBlockBlob cloudBlockBlob = _blob.GetBlockBlobReference(blobName);
            try
            {
                await cloudBlockBlob.DeleteAsync();
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 404)
                {
                    throw new ImageNotFoundException($"Image {imageID} not found");
                }
                throw new StorageErrorException($"Could delete image with id {imageID}", e);
            }
        }

        public async Task<byte[]> Download(string imageID)
        {
            string blobName = imageID;
            CloudBlockBlob cloudBlockBlob = _blob.GetBlockBlobReference(blobName);
            try
            {
                using (var stream = new MemoryStream())
                {
                    await cloudBlockBlob.DownloadToStreamAsync(stream);
                    return stream.ToArray();
                }
                    
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 404) 
                {
                    throw new ImageNotFoundException($"Image {imageID} not found");
                }
                throw new StorageErrorException("Could not write to Azure Table", e);
            }
        }

        public async Task<string> Upload(byte[] array)
        {
            string blobName = Guid.NewGuid().ToString();
            CloudBlockBlob cloudBlockBlob = _blob.GetBlockBlobReference(blobName);
            try
            {
                await cloudBlockBlob.UploadFromByteArrayAsync(array,0,array.Length);
                return blobName;
            }
            catch(StorageException e)
            {
                throw new StorageErrorException("Could not write to Azure Table", e);
            }

        }
    }
}
