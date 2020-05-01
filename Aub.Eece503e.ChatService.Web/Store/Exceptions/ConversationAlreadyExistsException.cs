using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class ConversationAlreadyExistsException : Exception
    {
        public ConversationAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
