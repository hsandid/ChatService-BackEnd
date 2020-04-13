using System;
namespace Aub.Eece503e.ChatService.Datacontracts

{
    public class CreateConversationResponse
    {
        public string Id { get; set; }
        public int CreatedUnixTime { get; set; }
    }
}
