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
        private readonly ILogger<ConversationsController> _logger;
        private readonly TelemetryClient _telemetryClient;

        public ConversationsController(IMessageStore messageStore, ILogger<ConversationsController> logger, TelemetryClient telemetryClient)
        {
            _messageStore = messageStore;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }


        [HttpGet("{conversationId}/messages/{messageId}")]
        public async Task<IActionResult> GetMessage(string conversationId, string messageId)
        {
            using (_logger.BeginScope("{Message_ID}", messageId))
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    MessageWithUnixTime message = await _messageStore.GetMessage(conversationId, messageId);
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
        public async Task<IActionResult> GetMessageList(string conversationId, string continuationToken, int limit)
        {
            using (_logger.BeginScope("{Conversation_ID}", conversationId))
            { 
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    MessageList messages = await _messageStore.GetMessages(conversationId, continuationToken, limit);
                    _telemetryClient.TrackMetric("MessageStore.GetMessages.Time", stopWatch.ElapsedMilliseconds);
                    MessageListResponse messagesResponse = new MessageListResponse
                    {
                        Messages = messages.Messages,
                        NextUri = $"api/conversations/{conversationId}/messages?continuationToken={WebUtility.UrlEncode(messages.ContinuationToken)}&limit={limit}"
                    };
                    return Ok(messagesResponse);
                }
                catch (MessagesNotFoundException e)
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
        public async Task<IActionResult> PostMessage(string conversationId, [FromBody] Message message)
        {
            using (_logger.BeginScope("{Message_ID}", message.Id))
            {
                if (!ValidateMessage(message, out string error))
                {
                    return BadRequest(error);
                }

                MessageWithUnixTime messageWithUnixTime = new MessageWithUnixTime
                {
                    Id = message.Id,
                    Text = message.Text,
                    SenderUsername = message.SenderUsername,
                    UnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    await _messageStore.AddMessage(messageWithUnixTime, conversationId);
                    _telemetryClient.TrackMetric("MessageStore.AddMessage.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("MessageAdded");
                    return CreatedAtAction(nameof(GetMessage), new { conversationId = conversationId, messageId = message.Id }, messageWithUnixTime);
                }
                catch (MessageAlreadyExistsException)
                {
                    return CreatedAtAction(nameof(GetMessage), new { conversationId = conversationId, messageId = message.Id }, messageWithUnixTime); ; //we agreed to return created if already exists.
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed add message {message.Id} to storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                { 
                    _logger.LogError(e, $"Unknown exception occured while adding message {message.Id} to storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }
        }

        private bool ValidateMessage(Message message, out string error)
        {
            if (string.IsNullOrWhiteSpace(message.Id))
            {
                error = "The message id must not be empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(message.Text))// The client app shouldn't allow empty messages. We can allow emtpy messages on our side.
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
    }
}