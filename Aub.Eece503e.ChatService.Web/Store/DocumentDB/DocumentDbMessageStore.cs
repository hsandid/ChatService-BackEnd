using System;
using System.Linq;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Aub.Eece503e.ChatService.Web.Store.DocumentDB
{
    public class DocumentDbMessageStore : IMessageStore
    {
        private readonly IDocumentClient _documentClient;
        private readonly IOptions<DocumentDbSettings> _options;

        private Uri DocumentCollectionUri =>
            UriFactory.CreateDocumentCollectionUri(_options.Value.DatabaseName, _options.Value.CollectionName);

        public DocumentDbMessageStore(IDocumentClient documentClient, IOptions<DocumentDbSettings> options)
        {
            _documentClient = documentClient;
            _options = options;
        }

        public async Task AddMessage(Message message, string conversationId)
        {
            try
            {
                var entity = ToEntity(conversationId, message);
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                if((int)e.StatusCode == 409)
                {
                    throw new MessageAlreadyExistsException($"Message {message.Id} already exists in storage");
                }

                throw new StorageErrorException($"Failed to add message {message.Id} to conversation {conversationId}", e);
            }
        }


        public async Task<MessageList> GetMessages(string conversationId, string continuationToken, int limit, long lastSeenMessageTime)
        {
            try
            {
                var feedOptions = new FeedOptions
                {
                    MaxItemCount = limit,
                    EnableCrossPartitionQuery = false,
                    RequestContinuation = continuationToken,
                    PartitionKey = new PartitionKey($"m_{conversationId}")
                };

                IQueryable<DocumentDbMessageEntity> query = _documentClient.CreateDocumentQuery<DocumentDbMessageEntity>(DocumentCollectionUri, feedOptions)
                    .OrderByDescending(entity => entity.UnixTime)
                    .Where(entity => entity.UnixTime > lastSeenMessageTime);
                FeedResponse<DocumentDbMessageEntity> feedResponse = await query.AsDocumentQuery().ExecuteNextAsync<DocumentDbMessageEntity>();
                return new MessageList
                {
                    ContinuationToken = feedResponse.ResponseContinuation,
                    Messages = feedResponse.Select(ToMessagesResponseEntry).ToArray()
                };
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 404)
                {
                    throw new ConversationNotFoundException($"ConversationId {conversationId} was not found in storage");
                }


                throw new StorageErrorException($"Failed to list messages in conversation {conversationId}", e);
            }
        }

        public async Task<Message> GetMessage(string conversationId, string messageId)
        {
            try
            {
                var entity = await _documentClient.ReadDocumentAsync<DocumentDbMessageEntity>(
                    CreateDocumentUri(messageId),
                    new RequestOptions { PartitionKey = new PartitionKey($"m_{conversationId}") });
                return ToMessage(entity);
            }
            catch (DocumentClientException e)
            {
                if((int)e.StatusCode == 404)
                {
                    throw new MessageNotFoundException($"Message {messageId} was not found in storage");
                }
            
                throw new StorageErrorException($"Failed to retrieve message {messageId} from conversation {conversationId}", e);
            }
        }


        private static DocumentDbMessageEntity ToEntity(string conversationId, Message message)
        {
            return new DocumentDbMessageEntity
            {
                PartitionKey = $"m_{conversationId}",
                Id = message.Id,
                Text = message.Text,
                SenderUsername = message.SenderUsername,
                UnixTime = message.UnixTime
            };
        }

        private static GetMessagesResponseEntry ToMessagesResponseEntry(DocumentDbMessageEntity entity)
        {
            return new GetMessagesResponseEntry
            { 
                Text = entity.Text,
                SenderUsername = entity.SenderUsername,
                UnixTime = entity.UnixTime
            };
        }

        private static Message ToMessage(DocumentDbMessageEntity entity)
        {
            return new Message
            {
                Id = entity.Id,
                Text = entity.Text,
                SenderUsername = entity.SenderUsername,
                UnixTime = entity.UnixTime
            };
        }

        class DocumentDbMessageEntity
        {
            public string PartitionKey { get; set; }
            [JsonProperty("id")]
            public string Id { get; set; }
            public string Text { get; set; }
            public string SenderUsername{ get; set; }
            public long UnixTime { get; set; }
        }

        private Uri CreateDocumentUri(string documentId)
        {
            return UriFactory.CreateDocumentUri(_options.Value.DatabaseName, _options.Value.CollectionName, documentId);
        }
    }
}
