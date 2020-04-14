using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class MessagesNotFoundException : Exception
    {
        public MessagesNotFoundException(string message) : base(message)
        {
        }
    }
}
