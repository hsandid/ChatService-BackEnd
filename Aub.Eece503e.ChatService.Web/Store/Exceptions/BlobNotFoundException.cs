using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class BlobNotFoundException : Exception
    {
        public BlobNotFoundException(string message) : base(message)
        {
        }
    }
}
