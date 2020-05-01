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
    public class DocumentDbConversationStore : IConversationStore
    {
        private readonly IDocumentClient _documentClient;
        private readonly IOptions<DocumentDbSettings> _options;

        private Uri DocumentCollectionUri =>
            UriFactory.CreateDocumentCollectionUri(_options.Value.DatabaseName, _options.Value.CollectionName);
        
        public DocumentDbConversationStore(IDocumentClient documentClient, IOptions<DocumentDbSettings> options)
        {
            _documentClient = documentClient;
            _options = options;
        }

        //TO-DO
        //Complete functions associated to DocumentDb for Conversations
        //Create DocumentDbConversationEntity
        //Create ToConversation, ToEntity, ToConversationResponseEntry

        //Should add conversation to both partitions
        //Edge case : Conversation only added to one of the partitions
        public async Task AddConversation(ConversationClass conversation)
        {
            string conversationId = $"{conversation.Participants[0] }_{ conversation.Participants[1]}";

            try
            {
                var entity = ToEntity(conversation.Participants[0], conversation);
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 409)
                {
                    throw new ConversationAlreadyExistsException($"Conversation {conversationId} already exists in partition {conversation.Participants[0]}");
                }

                throw new StorageErrorException($"Failed to add conversation {conversationId} to partition {conversation.Participants[0]}", e);
            }

            try
            {
                var entity = ToEntity(conversation.Participants[1], conversation);
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 409)
                {
                    throw new ConversationAlreadyExistsException($"Conversation {conversationId} already exists in partition {conversation.Participants[1]}");
                }

                throw new StorageErrorException($"Failed to add conversation {conversationId} to partition {conversation.Participants[1]}", e);
            }
        }


        public async Task<ConversationClass> GetConversation(string partitionKey, string conversationId)
        {
            try
            {
                var entity = await _documentClient.ReadDocumentAsync<DocumentDbConversationEntity>(
                    CreateDocumentUri(conversationId),
                    new RequestOptions { PartitionKey = new PartitionKey($"{partitionKey}")});
                return ToConversation(entity);
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 404)
                {
                    throw new ConversationNotFoundException($"Conversation {conversationId} was not found in partition {partitionKey}");
                }

                throw new StorageErrorException($"Failed to retrieve conversation {conversationId} from partition {partitionKey}", e);
            }
        }


        //Profile Not Found exception may be dangerous here
        public async Task<ConversationList> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            try
            {
                var feedOptions = new FeedOptions
                {
                    MaxItemCount = limit,
                    EnableCrossPartitionQuery = false,
                    RequestContinuation = continuationToken,
                    PartitionKey = new PartitionKey($"{username}")
                };

                IQueryable<DocumentDbConversationEntity> query = _documentClient.CreateDocumentQuery<DocumentDbConversationEntity>(DocumentCollectionUri, feedOptions)
                    .OrderByDescending(entity => entity.LastModifiedUnixTime)
                    .Where(entity => entity.LastModifiedUnixTime > lastSeenConversationTime);
                FeedResponse<DocumentDbConversationEntity> feedResponse = await query.AsDocumentQuery().ExecuteNextAsync<DocumentDbConversationEntity>();
                return new ConversationList
                {
                    ContinuationToken = feedResponse.ResponseContinuation,
                    Conversations = feedResponse.Select(ToConversationResponseEntry).ToArray()
                };
            }
            catch (DocumentClientException e)
            {
                if ((int)e.StatusCode == 404)
                {
                    throw new ProfileNotFoundException($"User {username} was not found in storage");
                }


                throw new StorageErrorException($"Failed to list conversations of user {username}", e);
            }
        }

        private static DocumentDbConversationEntity ToEntity(string partitionKey, ConversationClass conversation)
        {
            return new DocumentDbConversationEntity
            {
                PartitionKey = $"{partitionKey}",
                Id = $"{conversation.Participants[0]}_{conversation.Participants[1]}",
                Participants = conversation.Participants,
                LastModifiedUnixTime = conversation.LastModifiedUnixTime
            };
        }

        private static ConversationClass ToConversation(DocumentDbConversationEntity entity)
        {
            return new ConversationClass
            {
                Participants = entity.Participants,
                LastModifiedUnixTime = entity.LastModifiedUnixTime
            };
        }

        //REMOVE THIS, ONLY TEMPORARY !!!
        private static Profile _testProfile = new Profile
        {
            Username = "Hasaxc",
            Firstname = "John",
            Lastname = "Smith"
        };

        //FIND A WAY TO ATTACH PROFILE RECIPIENT
        private static GetConversationsResponseEntry ToConversationResponseEntry(DocumentDbConversationEntity entity)
        {
            return new GetConversationsResponseEntry
            {
                Id = entity.Id,
                LastModifiedUnixTime = entity.LastModifiedUnixTime,
                Recipient  = _testProfile
            };
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
