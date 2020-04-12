using System;
namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class MessageWithoutId
    {
        public string Text { get; set; }
        public string SenderUsername { get; set; }
        public long UnixTime { get; set; }
    }
}
