using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class ImageNotFoundException : Exception
    {
        public ImageNotFoundException(string message) : base(message)
        {
        }
    }
}
