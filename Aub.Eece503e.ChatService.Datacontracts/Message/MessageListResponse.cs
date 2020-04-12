namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class MessageListResponse
    {
        public MessageWithoutId[] Messages {get; set;}
        public string NextUri { get; set; }
    }
}
