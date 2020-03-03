using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IImageStore
    {
        Task<string> Upload(byte[] array);
        Task<byte[]> Download(string imageID);
        Task Delete(string imageID);
    }
}
