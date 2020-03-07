using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class BlobAlreadyExistsException : Exception
    {
        public BlobAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
