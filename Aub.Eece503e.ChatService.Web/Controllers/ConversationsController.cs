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

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversation(string conversationId)
        {
            using (_logger.BeginScope("{ConversationID}", conversationId))
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    PostConversationResponse conversation = await _conversationStore.GetConversation(conversationId);
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


        [HttpPost]
        public async Task<IActionResult> PostConversation([FromBody] PostConversationRequest postConversationRequest)
        {
            var conversationId = postConversationRequest.Participants[0];
            if (string.Compare(postConversationRequest.Participants[0], postConversationRequest.Participants[1]) == -1)
            {
                conversationId += $"_{postConversationRequest.Participants[1]}";
            }
            else
            {
                conversationId = $"{postConversationRequest.Participants[1]}_" + conversationId;
            }
            using (_logger.BeginScope("{ConversationID}", conversationId))
            {
                if (!ValidateConversation(postConversationRequest, out string error1))
                {
                    return BadRequest(error1);
                }
                if(!ValidateMessage(postConversationRequest.FirstMessage, out string error2))
                {
                    return BadRequest(error2);
                }

                Message message = new Message
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
                    await _conversationStore.AddConversation(conversation);
                    _telemetryClient.TrackMetric("ConversationStore.AddConversation.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("ConversationAdded");
                    return CreatedAtAction(nameof(GetConversation), new { conversationId = conversationId}, conversation);
                }
                catch (MessageAlreadyExistsException)
                {
                    await _conversationStore.AddConversation(conversation);
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


        [HttpGet("{conversationId}/messages/{messageId}")]
        public async Task<IActionResult> GetMessage(string conversationId, string messageId)
        {
            using (_logger.BeginScope("{MessageID}", messageId))
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    Message message = await _messageStore.GetMessage(conversationId, messageId);
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

        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> PostMessage(string conversationId, [FromBody] PostMessageRequest postMessageRequest)
        {
            using (_logger.BeginScope("{Message_ID}", postMessageRequest.Id))
            {
                if (!ValidateMessage(postMessageRequest, out string error))
                {
                    return BadRequest(error);
                }

                Message message = new Message
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
                    return CreatedAtAction(nameof(GetMessage), new { conversationId = conversationId, messageId = postMessageRequest.Id }, originalMessage); ; //we agreed to return already exisitng message if it exists.
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
