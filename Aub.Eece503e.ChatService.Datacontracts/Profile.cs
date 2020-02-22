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

        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Username);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Firstname);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Lastname);
            return hashCode;
        }
    }
}
