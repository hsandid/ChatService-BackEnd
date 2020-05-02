using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System.Net;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;

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
                var stopWatch = Stopwatch.StartNew();
                var fetchedMessage = await _messageStore.AddMessage(message, conversationId);
                await _conversationStore.UpdateConversation(conversationId, fetchedMessage.UnixTime);
                _telemetryClient.TrackMetric("MessageStore.AddMessage.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("MessageAdded");
                return fetchedMessage;
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
                var stopWatch = Stopwatch.StartNew();
                 await _messageStore.AddMessage(message, conversationId);
                var fetchedConversation = await _conversationStore.AddConversation(conversation, postConversationRequest.Participants);
                _telemetryClient.TrackMetric("ConversationStore.AddConversation.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("ConversationAdded");
                return fetchedConversation;
            }
        }
        public async Task<GetConversationsResponse> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                    var stopWatch = Stopwatch.StartNew();
                    ConversationList conversations = await _conversationStore.GetConversations(username, continuationToken, limit, lastSeenConversationTime);
                    _telemetryClient.TrackMetric("ConversationStore.GetConversations.Time", stopWatch.ElapsedMilliseconds);
                    string nextUri = "";
                    if (!string.IsNullOrWhiteSpace(conversations.ContinuationToken))
                    {
                        nextUri = $"api/conversations?username={username}&continuationToken={WebUtility.UrlEncode(conversations.ContinuationToken)}&limit={limit}&lastSeenConversationTime={lastSeenConversationTime}";
                    }

                    string recipientUsername;
                    Profile recipient;
                    List<GetConversationsResponseEntry> conversationEntries = new List<GetConversationsResponseEntry>();
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
                        try
                        {
                            recipient = await _profileStore.GetProfile(recipientUsername);
                             conversationEntries.Add(new GetConversationsResponseEntry
                             {
                                LastModifiedUnixTime = conversations.Conversations[index].LastModifiedUnixTime,
                                 Id = conversations.Conversations[index].Id,
                                 Recipient = recipient
                             }
                                );
                        }
                        catch(ProfileNotFoundException)
                        {
                            //Disregard this profile because it is now not existing.
                        }
                    }


                    GetConversationsResponse conversationsResponse = new GetConversationsResponse
                    {
                        Conversations = conversationEntries.ToArray(),
                        NextUri = nextUri
                    };

                    return conversationsResponse;
                
            }
        }
    }
}
