using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class MessageNotFoundException: Exception
    {
        public MessageNotFoundException(string message): base(message)
        {
        }
    }
}
