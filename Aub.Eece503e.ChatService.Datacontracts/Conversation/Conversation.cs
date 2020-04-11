using System;
namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class Conversation
    {
        public string Id { get; set; }
        public int LastModifiedUnixTime { get; set; }
        public Profile Recepient { get; set; }
    }
}
