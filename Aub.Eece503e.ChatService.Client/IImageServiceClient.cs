using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Client
{
    public interface IImageServiceClient
    {
        Task<UploadImageResponse> UploadImage(Stream stream);
        Task<DownloadImageResponse> DownloadImage(string imageId);
        Task DeleteImage(string imageId);
    }
}
