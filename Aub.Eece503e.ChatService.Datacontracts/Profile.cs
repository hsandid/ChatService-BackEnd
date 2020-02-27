using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class Profile
    {
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Profile profile &&
                   Username == profile.Username &&
                   Firstname == profile.Firstname &&
                   Lastname == profile.Lastname;
        }

    }
}
