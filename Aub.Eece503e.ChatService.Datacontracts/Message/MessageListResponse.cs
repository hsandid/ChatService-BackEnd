using System;
namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class MessageListResponse
    {
        public MessageListResponseItem[] Messages {get; set;}
        public string NextURI { get; set; }
    }
}
