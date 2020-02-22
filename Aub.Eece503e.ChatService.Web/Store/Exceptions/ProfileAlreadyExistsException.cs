using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class ProfileAlreadyExistsException: Exception
    {
        public ProfileAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
