using System;
namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class GetConversationsResponseEntry
    {
        public string Id { get; set; }
        public long LastModifiedUnixTime { get; set; }
        public Profile Recipient { get; set; }
    }
}
