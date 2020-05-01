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
        
        public async Task<PostConversationResponse> AddConversation(PostConversationResponse conversation, string[] participants)
        {
                Task<PostConversationResponse> task1 = AddConversationToPartition(conversation, participants, participants[0]);
                Task<PostConversationResponse> task2 = AddConversationToPartition(conversation, participants, participants[1]);
                await Task.WhenAll(task1, task2);

            //Here we have the choice to return task1.Result or task2.Result.
            //In most cases, it won't matter.
            //In an edge case where only one conversation fails. The client will try to create this conversation again.
            //In this case, the one that failed first will have a more recent created time.
            // We chose to always return the oldest for no technical reason.
            return task1.Result.CreatedUnixTime <= task2.Result.CreatedUnixTime ? task1.Result : task2.Result;
        }

        private async Task<PostConversationResponse> AddConversationToPartition(PostConversationResponse conversation, string[] participants, string username)
        {
            try
            {
                var entity = ToEntity(conversation, participants, username);
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
                return conversation;
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 409)
                {
                    return await GetConversation(conversation.Id, username);
                }

                throw new StorageErrorException($"Failed to add conversation {conversation.Id} to user {username}", e);
            }

        }

        private async Task<PostConversationResponse> GetConversation(string conversationId, string username)
        {
            try
            {
                var entity = await _documentClient.ReadDocumentAsync<DocumentDbConversationEntity>(
                    CreateDocumentUri(conversationId),
                    new RequestOptions { PartitionKey = new PartitionKey($"c_{username}") });
                return ToConversation(entity);
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 404)
                {
                    throw new ConversationNotFoundException($"Conversation {conversationId} was not found in partition of user {username}");
                }

                throw new StorageErrorException($"Failed to retrieve conversation {conversationId} from user partition {username}", e);
            }
        }
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

        public async Task UpdateConversation(string conversationId, long lastModifiedTime)
        {
            try
            {
                PostConversationResponse conversation = new PostConversationResponse
                {
                    Id = conversationId,
                    CreatedUnixTime = lastModifiedTime
                };
                string username1 = conversationId.Substring(0, conversationId.IndexOf('_'));
                string username2 = conversationId.Substring(conversationId.IndexOf('_') + 1, conversationId.Length - conversationId.IndexOf('_') - 1);
                string[] participants = { username1, username2 };
                var entity1 = ToEntity(conversation,participants, username1);
                var entity2 = ToEntity(conversation, participants, username2);
                Task task1 = _documentClient.UpsertDocumentAsync(DocumentCollectionUri, entity1);
                Task task2 = _documentClient.UpsertDocumentAsync(DocumentCollectionUri, entity1);
                await Task.WhenAll(task1, task2);
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 404)
                {
                    throw new ConversationNotFoundException($"Conversation with conversation Id {conversationId} was not found in storage");
                }

                throw new StorageErrorException($"Failed to update conversation {conversationId}", e);
            }
        }
    }
}
