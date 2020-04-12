using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class MessageAlreadyExistsException : Exception
    {
        public MessageAlreadyExistsException(string message): base(message)
        {
        }
    }
}
