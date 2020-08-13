using System;
using System.Collections.Generic;
using System.Text;

namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class ConversationListEntry
    {
        public string Id { get; set; }
        public long LastModifiedUnixTime { get; set; }
        public string[] Participants { get; set; }
    }
}
