namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class GetMessagesResponse
    {
        public GetMessagesResponseEntry[] Messages {get; set;}
        public string NextUri { get; set; }
    }
}
