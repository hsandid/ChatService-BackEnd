namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class MessageList
    {
        public GetMessagesResponseEntry[] Messages { get; set; }
        public string ContinuationToken { get; set; }
    }
}
