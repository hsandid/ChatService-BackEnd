using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public class InMemoryMessageStore: IMessageStore
    {
        private ConcurrentDictionary<string, MessageWithUnixTime> _messages = new ConcurrentDictionary<string, MessageWithUnixTime>();

        private static string GetKey(string conversationId, string messageId)
        {
            return $"{conversationId}_{messageId}";
        }

        public Task<MessageWithUnixTime> AddMessage(Message message, string conversationId)
        {
            string key = GetKey(conversationId, message.Id);

            MessageWithUnixTime messageWithUnixTime = new MessageWithUnixTime
            {
                Id = message.Id,
                Text = message.Text,
                SenderUsername = message.SenderUsername,
                UnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };


            if (!_messages.TryAdd(key, messageWithUnixTime))
            {
                throw new StorageErrorException($"message {message.Id} already exists.");
            }
            return Task.FromResult(messageWithUnixTime);
        }

        public Task<MessageList> GetMessages(string conversationId, string continuationToken, int limit)
        {
            int offset = 0;
            if (!string.IsNullOrEmpty(continuationToken))
            {
                offset = int.Parse(continuationToken);
            }

            var keys = _messages.Keys.Where(key => key.StartsWith($"{conversationId}_"));
            var messages = keys.Select(key => _messages[key]).ToList();
            messages.Sort((first, second) => second.UnixTime.CompareTo(first.UnixTime));
            messages = messages.Skip(offset).ToList();

            if (messages.Count <= limit)
            {
                return Task.FromResult(
                    new MessageList
                    {
                        Messages = messages.ToArray(),
                        ContinuationToken = null

                    });
            }

            return Task.FromResult(
                new MessageList
                {
                    ContinuationToken = (offset + limit).ToString(),
                    Messages = messages.Take(limit).ToArray()
                });
        }
    }
}
