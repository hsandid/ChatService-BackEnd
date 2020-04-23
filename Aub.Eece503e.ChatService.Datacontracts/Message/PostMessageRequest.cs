using System;
namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class PostMessageRequest
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string SenderUsername { get; set; }
    }
}
