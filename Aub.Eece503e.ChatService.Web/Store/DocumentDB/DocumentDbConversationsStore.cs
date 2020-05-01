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
    public class DocumentDbConversationsStore : IConversationStore
    {
        private readonly IDocumentClient _documentClient;
        private readonly IOptions<DocumentDbSettings> _options;

        private Uri DocumentCollectionUri =>
            UriFactory.CreateDocumentCollectionUri(_options.Value.DatabaseName, _options.Value.CollectionName);

        public DocumentDbConversationsStore(IDocumentClient documentClient, IOptions<DocumentDbSettings> options)
        {
            _documentClient = documentClient;
            _options = options;
        }

        //public async Task UpdateConversation(PostConversationResponse conversation, string[] participants,  long updatedConversationTime)
        //{
            
        //}
        // ADDITIONAL : Concerning the ConversationNotFoundException, should we check if the username exists ? It's fine if there are no conversations associated to the user as we can return
        // an empty array
        public async Task<ConversationList> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            try
            {
                var feedOptions = new FeedOptions
                {
                    MaxItemCount = limit,
                    EnableCrossPartitionQuery = false,
                    RequestContinuation = continuationToken,
                    PartitionKey = new PartitionKey($"c_{username}")
                };

                IQueryable<DocumentDbConversationEntity> query = _documentClient.CreateDocumentQuery<DocumentDbConversationEntity>(DocumentCollectionUri, feedOptions)
                    .OrderByDescending(entity => entity.LastModifiedUnixTime)
                    .Where(entity => entity.LastModifiedUnixTime > lastSeenConversationTime);
                FeedResponse<DocumentDbConversationEntity> feedResponse = await query.AsDocumentQuery().ExecuteNextAsync<DocumentDbConversationEntity>();
                return new ConversationList
                {
                    ContinuationToken = feedResponse.ResponseContinuation,
                    Conversations = feedResponse.Select(ToConversationsListEntry).ToArray()
                };
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 404)
                {
                    throw new ConversationNotFoundException($"No conversations associated to user {username} were found in storage");
                }


                throw new StorageErrorException($"Failed to list conversations of user {username}", e);
            }
        }
        private static ConversationListEntry ToConversationsListEntry(DocumentDbConversationEntity entity)
        {
            return new ConversationListEntry
            {
                Id = entity.Id,
                LastModifiedUnixTime = entity.LastModifiedUnixTime,
                Participants = entity.Participants
            };
        }

        //Some interesting questions regarding this function
        //We can address the edge cases by throwing a specific exception here, and addressing it on an upper layer (i.e. service/controller...)
        //Interesting ! If a conversation already exists, we can make use of GetConversation and only get that previous conversation which has already been added, with the correct UnixTime at creation
        //Dealing with Edge cases should be left for later, we have to get something working first
        public async Task AddConversation(PostConversationResponse conversation, string[] participants)
        {
            try
            {
                var entity = ToEntity(conversation, participants, participants[0]);
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 409)
                {
                    throw new ConversationAlreadyExistsException($"Conversation {conversation.Id} already exists in storage");
                }

                throw new StorageErrorException($"Failed to add conversation {conversation.Id} to user {participants[0]}", e);
            }

            try
            {
                var entity = ToEntity(conversation, participants, participants[1]);
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 409)
                {
                    throw new ConversationAlreadyExistsException($"Conversation {conversation.Id} already exists in storage");
                }

                throw new StorageErrorException($"Failed to add conversation {conversation.Id} to user {participants[1]}", e);
            }


        }

        //We can use this function to check if a conversation exists in the two partitions associated to each participant
        //We can modify it to do help us address edge cases, like making it throw more specific exceptions which can be addressed on the storage layer
        //Also, returning on the second call is okay or not ?
        public async Task<PostConversationResponse> GetConversation(string conversationId, string[] participants)
        {
            try
            {
                var entity = await _documentClient.ReadDocumentAsync<DocumentDbConversationEntity>(
                    CreateDocumentUri(conversationId),
                    new RequestOptions { PartitionKey = new PartitionKey($"c_{participants[0]}") });
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 404)
                {
                    throw new ConversationNotFoundException($"Conversation {conversationId} was not found in partition of user {participants[0]}");
                }

                throw new StorageErrorException($"Failed to retrieve conversation {conversationId} from user partition {participants[0]}", e);
            }

            try
            {
                var entity = await _documentClient.ReadDocumentAsync<DocumentDbConversationEntity>(
                    CreateDocumentUri(conversationId),
                    new RequestOptions { PartitionKey = new PartitionKey($"c_{participants[1]}") });
                return ToConversation(entity);
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 404)
                {
                    throw new ConversationNotFoundException($"Conversation {conversationId} was not found in partition of user {participants[1]}");
                }

                throw new StorageErrorException($"Failed to retrieve conversation {conversationId} from user partition {participants[1]}", e);
            }
        }

        //In this case, shouldn't CreatedUnixTime only be LastModifiedUnixTime ?
        private static PostConversationResponse ToConversation(DocumentDbConversationEntity entity)
        {
            return new PostConversationResponse
            {
                Id = entity.Id,
                CreatedUnixTime = entity.LastModifiedUnixTime
            };
        } 
        DocumentDbConversationEntity ToEntity(PostConversationResponse conversation, string[] participants, string userPartition)
        {
            DocumentDbConversationEntity entity = new DocumentDbConversationEntity
            {
                PartitionKey = $"c_{userPartition}",
                Id = $"{conversation.Id}",
                Participants = participants,
                LastModifiedUnixTime = conversation.CreatedUnixTime
            };
            return entity;
        }
        class DocumentDbConversationEntity
        {
            public string PartitionKey { get; set; }
            [JsonProperty("id")]
            public string Id { get; set; }
            public string[] Participants { get; set; }
            public long LastModifiedUnixTime { get; set; }
        }
        private Uri CreateDocumentUri(string documentId)
        {
            return UriFactory.CreateDocumentUri(_options.Value.DatabaseName, _options.Value.CollectionName, documentId);
        }
    }
}
