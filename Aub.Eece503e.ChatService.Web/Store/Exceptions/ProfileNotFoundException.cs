using System;
namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class ProfileNotFoundException: Exception
    {
        public ProfileNotFoundException(string message) : base(message)
        {
        }
    }
}
