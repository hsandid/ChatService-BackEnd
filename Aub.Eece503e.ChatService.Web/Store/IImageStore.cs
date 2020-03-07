using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IImageStore
    {
        Task<string> Upload(byte[] imageData);
        Task<byte[]> Download(string imageId);
        Task Delete(string imageId);
    }
}
