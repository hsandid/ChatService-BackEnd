using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System.Net;
namespace Aub.Eece503e.ChatService.Web.Services
{
    public class ConversationsService : IConversationsService
    {
        private readonly IProfileStore _profileStore;
        private readonly IMessageStore _messageStore;
        private readonly IConversationStore _conversationStore;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<ConversationsService> _logger;

        public ConversationsService(IMessageStore messageStore, IConversationStore conversationStore, IProfileStore profileStore, TelemetryClient telemetryClient, ILogger<ConversationsService> logger)
        {
            _messageStore = messageStore;
            _conversationStore = conversationStore;
            _profileStore = profileStore;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        //Concerning Exceptions, how can we 'propogate' them from the storage layer to the service layer and up to the controller ?
        //Seems like there should not be any exceptions on this layer
        //Should we assign the metrics here to ProfileStore.GetProfile.Time ?
        public async Task<Profile> GetProfileInformation(string username)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                    var stopWatch = Stopwatch.StartNew();
                    Profile profile = await _profileStore.GetProfile(username);
                    _telemetryClient.TrackMetric("ProfileStore.GetProfile.Time", stopWatch.ElapsedMilliseconds);
                    return profile;
            }
        }

        //We should add a function to update the conversations in each partition accordingly
        public async Task<PostMessageResponse> PostMessage(string conversationId, PostMessageRequest postMessageRequest)
        {
            using (_logger.BeginScope("{Message_ID}", postMessageRequest.Id))
            {            
                PostMessageResponse message = new PostMessageResponse
                {
                    Id = postMessageRequest.Id,
                    Text = postMessageRequest.Text,
                    SenderUsername = postMessageRequest.SenderUsername,
                    UnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    await _messageStore.AddMessage(message, conversationId);
                    _telemetryClient.TrackMetric("MessageStore.AddMessage.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("MessageAdded");
                    return message;
                }
                catch (MessageAlreadyExistsException)
                {
                    var originalMessage = await _messageStore.GetMessage(conversationId, message.Id);
                    return originalMessage;
                }
        }
        }
        public async Task<PostMessageResponse> GetMessage(string conversationId, string messageId)
        {
            using (_logger.BeginScope("{MessageID}", messageId))
            {
                    var stopWatch = Stopwatch.StartNew();
                    PostMessageResponse message = await _messageStore.GetMessage(conversationId, messageId);
                    _telemetryClient.TrackMetric("MessageStore.GetMessage.Time", stopWatch.ElapsedMilliseconds);
                    return message;
            }
        }
        public async Task<GetMessagesResponse> GetMessageList(string conversationId, string continuationToken, int limit, long lastSeenMessageTime)
        {
            using (_logger.BeginScope("{ConversationID}", conversationId))
            {
                var stopWatch = Stopwatch.StartNew();
                MessageList messages = await _messageStore.GetMessages(conversationId, continuationToken, limit, lastSeenMessageTime);
                _telemetryClient.TrackMetric("MessageStore.GetMessages.Time", stopWatch.ElapsedMilliseconds);
                GetMessagesResponse messageList;
                String nextUri = "";
                if (!string.IsNullOrWhiteSpace(messages.ContinuationToken))
                {
                    nextUri = $"api/conversations/{conversationId}/messages?continuationToken={WebUtility.UrlEncode(messages.ContinuationToken)}&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}";
                }

                messageList = new GetMessagesResponse
                {
                    Messages = messages.Messages,
                    NextUri = nextUri
                };

                return messageList;
            }
        }

        public async Task<PostConversationResponse> PostConversation(PostConversationRequest postConversationRequest,string conversationId)
        {
            using (_logger.BeginScope("{ConversationID}", conversationId))
            {
                PostMessageResponse message = new PostMessageResponse
                {
                    Id = postConversationRequest.FirstMessage.Id,
                    Text = postConversationRequest.FirstMessage.Text,
                    SenderUsername = postConversationRequest.FirstMessage.SenderUsername,
                    UnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                PostConversationResponse conversation = new PostConversationResponse
                {
                    Id = conversationId,
                    CreatedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    await _messageStore.AddMessage(message, conversationId);
                    await _conversationStore.AddConversation(conversation, postConversationRequest.Participants);
                    _telemetryClient.TrackMetric("ConversationStore.AddConversation.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("ConversationAdded");
                    return conversation;
                }
                catch (MessageAlreadyExistsException)
                {
                    //Nested try/catch block. Is it a good idea ?
                    try
                    {
                        //Should we call the store directly or pass through the service function?
                        await _conversationStore.AddConversation(conversation, postConversationRequest.Participants);
                        return conversation;
                    }
                    catch (ConversationAlreadyExistsException)
                    {
                        var fetchedConversation = await _conversationStore.GetConversation(conversation.Id, postConversationRequest.Participants);
                        return fetchedConversation;
                    }
                    
                    
                }
                catch (ConversationAlreadyExistsException)
                {
                    var fetchedConversation = await _conversationStore.GetConversation(conversation.Id, postConversationRequest.Participants);
                    return fetchedConversation;
                }

            }
        }
        public async Task<PostConversationResponse> GetConversation(string conversationId, string[] participants)
        {
            using (_logger.BeginScope("{ConversationID}", conversationId))
            {
                    var stopWatch = Stopwatch.StartNew();
                    PostConversationResponse conversation = await _conversationStore.GetConversation(conversationId, participants);
                    _telemetryClient.TrackMetric("ConversationStore.GetConversation.Time", stopWatch.ElapsedMilliseconds);
                    return conversation; 
            }
        }
        public async Task<GetConversationsResponse> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                    var stopWatch = Stopwatch.StartNew();
                    ConversationList conversations = await _conversationStore.GetConversations(username, continuationToken, limit, lastSeenConversationTime);
                    _telemetryClient.TrackMetric("ConversationStore.GetConversations.Time", stopWatch.ElapsedMilliseconds);
                    String nextUri = "";
                    if (!string.IsNullOrWhiteSpace(conversations.ContinuationToken))
                    {
                        nextUri = $"api/conversations?username={username}&continuationToken={WebUtility.UrlEncode(conversations.ContinuationToken)}&limit={limit}&lastSeenMessageTime={lastSeenConversationTime}";
                    }

                    string recipientUsername = "";
                    Profile recipient;
                    GetConversationsResponseEntry[] conversationEntries = new GetConversationsResponseEntry[conversations.Conversations.Length];
                    for (int index = 0; index < conversations.Conversations.Length; index++)
                    {
                        if (conversations.Conversations[index].Participants[0] != username)
                        {
                            recipientUsername = conversations.Conversations[index].Participants[0];
                        }
                        else
                        {
                            recipientUsername = conversations.Conversations[index].Participants[1];
                        }

                        recipient = await GetProfileInformation(recipientUsername);
                        conversationEntries[index] = new GetConversationsResponseEntry
                        {
                            LastModifiedUnixTime = conversations.Conversations[index].LastModifiedUnixTime,
                            Id = conversations.Conversations[index].Id,
                            Recipient = recipient
                        };

                    }


                    GetConversationsResponse conversationsResponse = new GetConversationsResponse
                    {
                        Conversations = conversationEntries,
                        NextUri = nextUri
                    };

                    return conversationsResponse;
                
            }
        }
    }
}
