namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class MessageListResponse
    {
        public MessageWithUnixTime[] Messages {get; set;}
        public string NextUri { get; set; }
    }
}
