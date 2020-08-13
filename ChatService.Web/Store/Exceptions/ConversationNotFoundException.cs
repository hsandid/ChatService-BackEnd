using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class ConversationNotFoundException : Exception
    {
        public ConversationNotFoundException(string message) : base(message)
        {
        }
    }
}
