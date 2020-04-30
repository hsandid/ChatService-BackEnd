using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class ConversationAlreadyExists : Exception
    {
        public ConversationAlreadyExists(string message) : base(message)
        {
        }
    }
}
