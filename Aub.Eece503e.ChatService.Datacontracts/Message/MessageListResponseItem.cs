using System;
namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class MessageListResponseItem
    {
        public string Text { get; set; }
        public string SenderUsername { get; set; }
        public int UnixTime { get; set; }
    }
}
