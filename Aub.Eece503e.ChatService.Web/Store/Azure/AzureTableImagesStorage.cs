using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
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

        public Task Delete(string imageID)
        {
            throw new System.NotImplementedException();
        }

        public Task<byte[]> Download(string imageID)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> Upload(byte[] array)
        {
            throw new System.NotImplementedException();
        }
    }
}
