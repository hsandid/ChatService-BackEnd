namespace Aub.Eece503e.ChatService.Datacontracts
{
    public class PostMessageResponse
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string SenderUsername { get; set; }
        public long UnixTime { get; set; }

        public override bool Equals(object obj)
        {
            return obj is PostMessageResponse message &&
                   Id == message.Id &&
                   Text == message.Text &&
                   SenderUsername == message.SenderUsername;
        }
    }
}

