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

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly IMessageStore _messageStore;
        private readonly IConversationStore _conversationStore;
        private readonly ILogger<ConversationsController> _logger;
        private readonly TelemetryClient _telemetryClient;

        public ConversationsController(IMessageStore messageStore, IConversationStore conversationStore, ILogger<ConversationsController> logger, TelemetryClient telemetryClient)
        {
            _messageStore = messageStore;
            _conversationStore = conversationStore;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        //This one is not called directly by the user, why not move it to the service layer
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversation(string conversationId, string[] participants)
        {
            using (_logger.BeginScope("{ConversationID}", conversationId))
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    PostConversationResponse conversation = await _conversationStore.GetConversation(conversationId, participants);
                    _telemetryClient.TrackMetric("ConversationStore.GetConversation.Time", stopWatch.ElapsedMilliseconds);
                    return Ok(conversation);
                }
                catch (ConversationNotFoundException e)
                {
                    _logger.LogError(e, $"Conversation {conversationId} was not found in storage");
                    return NotFound($"The conversation with Id {conversationId} was not found");
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed to retrieve conversation {conversationId} from storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while retrieving conversation {conversationId} from storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }
        }

        //We can attempt to handle exceptions thrown at the storage layer here.
        //We are already catching MessageAlreadyExistsException, why not catch ConversationAlreadyExistsException and address edge cases accordingly ?
        [HttpPost]
        public async Task<IActionResult> PostConversation([FromBody] PostConversationRequest postConversationRequest)
        {
            string conversationId = postConversationRequest.Participants[0] + "_" + postConversationRequest.Participants[1];
            using (_logger.BeginScope("{ConversationID}", conversationId))
            {
                if (!ValidateConversation(postConversationRequest, out string conversationFormatError))
                {
                    return BadRequest(conversationFormatError);
                }
                if (!ValidateMessage(postConversationRequest.FirstMessage, out string messageFormatError))
                {
                    return BadRequest(messageFormatError);
                }

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
                    return CreatedAtAction(nameof(GetConversation), new { conversationId = conversationId }, conversation);
                }
                catch (MessageAlreadyExistsException)
                {
                    await _conversationStore.AddConversation(conversation, postConversationRequest.Participants);
                    return CreatedAtAction(nameof(GetConversation), new { conversationId = conversationId }, conversation);
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed to add {conversationId} to storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while adding conversation {conversationId} to storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    ConversationList conversations = await _conversationStore.GetConversations(username, continuationToken, limit, lastSeenConversationTime);
                    _telemetryClient.TrackMetric("ConversationStore.GetConversations.Time", stopWatch.ElapsedMilliseconds);
                    String nextUri = "";
                    if (!string.IsNullOrWhiteSpace(conversations.ContinuationToken))
                    {
                        nextUri = $"api/conversations/{username}/messages?continuationToken={WebUtility.UrlEncode(conversations.ContinuationToken)}&limit={limit}&lastSeenMessageTime={lastSeenConversationTime}";
                    }

                    //Need to add an extra layer (services ?) to take care of all calls which are not directly tied to the controller
                    //ADDING A TEMPORARY PROFILE FOR EACH CONVERSATION, THIS HAS TO BE REMOVED AND LEGITIMISED AS SOON AS THE SERVICE LAYER IS UP !!!!!!

                    ///TO REMOVE !!!!
                    GetConversationsResponseEntry[] test = new GetConversationsResponseEntry[conversations.Conversations.Length];
                    for (int i=0;i<conversations.Conversations.Length;i++)
                    {
                        test[i] = new GetConversationsResponseEntry
                        { 
                            LastModifiedUnixTime = conversations.Conversations[i].LastModifiedUnixTime,
                            Id = conversations.Conversations[i].Id,
                            Recipient = new Profile { Firstname="Joe",Lastname="Regan",ProfilePictureId="kek",Username="hxa04"}
                        };

                    }

                    
                    GetConversationsResponse conversationsResponse = new GetConversationsResponse
                    {
                        Conversations = test,
                        NextUri = nextUri
                    };
                    ///TO REMOVE END

                    return Ok(conversationsResponse);
                }
                catch (ConversationNotFoundException e)
                {
                    _logger.LogError(e, $"No conversations associated to user {username} were found in storage");
                    return NotFound($"No conversations associated to user  {username} were found");
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed to retrieve conversations associated to user {username} from storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while retrieving conversations of user {username} from storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }

        }

        [HttpGet("{conversationId}/messages/{messageId}")]
        public async Task<IActionResult> GetMessage(string conversationId, string messageId)
        {
            using (_logger.BeginScope("{MessageID}", messageId))
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    PostMessageResponse message = await _messageStore.GetMessage(conversationId, messageId);
                    _telemetryClient.TrackMetric("MessageStore.GetMessage.Time", stopWatch.ElapsedMilliseconds);
                    return Ok(message);
                }
                catch (MessageNotFoundException e)
                {
                    _logger.LogError(e, $"Message {messageId} was not found in storage");
                    return NotFound($"The message with messageId {messageId} was not found");
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed to retrieve message {messageId} from storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while retrieving message {messageId} from storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessageList(string conversationId, string continuationToken, int limit, long lastSeenMessageTime)
        {
            using (_logger.BeginScope("{ConversationID}", conversationId))
            { 
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    MessageList messages = await _messageStore.GetMessages(conversationId, continuationToken, limit, lastSeenMessageTime);
                    _telemetryClient.TrackMetric("MessageStore.GetMessages.Time", stopWatch.ElapsedMilliseconds);
                    GetMessagesResponse messagesResponse;
                    String nextUri = "";
                    if (!string.IsNullOrWhiteSpace(messages.ContinuationToken))
                    {
                        nextUri = $"api/conversations/{conversationId}/messages?continuationToken={WebUtility.UrlEncode(messages.ContinuationToken)}&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}";
                    }

                    messagesResponse = new GetMessagesResponse
                    {
                        Messages = messages.Messages,
                        NextUri = nextUri
                    };
                   
                    return Ok(messagesResponse);
                }
                catch (ConversationNotFoundException e)
                {
                    _logger.LogError(e, $"Conversation {conversationId} was not found in storage");
                    return NotFound($"The conversation with conversatioId {conversationId} was not found");
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed to retrieve messages in conversation {conversationId} from storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while retrieving messages in {conversationId} from storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }

        }

        //ADDITIONAL :We should check if the conversation exists here before adding any message
        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> PostMessage(string conversationId, [FromBody] PostMessageRequest postMessageRequest)
        {
            using (_logger.BeginScope("{Message_ID}", postMessageRequest.Id))
            {
                if (!ValidateMessage(postMessageRequest, out string error))
                {
                    return BadRequest(error);
                }

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
                    return CreatedAtAction(nameof(GetMessage), new { conversationId = conversationId, messageId = postMessageRequest.Id }, message);
                }
                catch (MessageAlreadyExistsException)
                {
                    var originalMessage = await _messageStore.GetMessage(conversationId, message.Id);
                    return CreatedAtAction(nameof(GetMessage), new { conversationId = conversationId, messageId = postMessageRequest.Id }, originalMessage);
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed add message {postMessageRequest.Id} to storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while adding message {postMessageRequest.Id} to storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }
        }

        private bool ValidateMessage(PostMessageRequest message, out string error)
        {
            if (string.IsNullOrWhiteSpace(message.Id))
            {
                error = "The message id must not be empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(message.Text))// The client app shouldn't allow empty messages. We can choose to allow emtpy messages on our side.
            {
                error = "The message text must not be empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(message.SenderUsername))
            {
                error = "The message sender username must not be empty";
                return false;
            }
            error = "";
            return true;
        }

        private bool ValidateConversation(PostConversationRequest conversation, out string error)
        {
            if (conversation.Participants.Length != 2)
            {
                error = "Conversatoin participants must include 2 usernames";
                return false;
            }
            if (string.IsNullOrWhiteSpace(conversation.Participants[0]) || string.IsNullOrWhiteSpace(conversation.Participants[1]))// The client app shouldn't allow empty messages. We can choose to allow emtpy messages on our side.
            {
                error = "Both participants must not be empty";
                return false;
            }
            error = "";
            return true;
        }
    }
}
