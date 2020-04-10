using System;
namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class ConversationListResponse
    {
        public Conversation[] Conversations { get; set; }
        public string NextURI { get; set; }
    }
}
