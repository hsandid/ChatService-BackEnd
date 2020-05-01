using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Services;
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
        private readonly IConversationsService _conversationsService;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(IConversationsService conversationsService, ILogger<ConversationsController> logger, TelemetryClient telemetryClient)
        {
            _conversationsService = conversationsService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostConversation([FromBody] PostConversationRequest postConversationRequest)
        {
            if (!ValidateConversation(postConversationRequest, out string conversationFormatError))
            {
                return BadRequest(conversationFormatError);
            }
            if (!ValidateMessage(postConversationRequest.FirstMessage, out string messageFormatError))
            {
                return BadRequest(messageFormatError);
            }

            string conversationId = postConversationRequest.Participants[0];
            if(String.Compare(postConversationRequest.Participants[0], postConversationRequest.Participants[1]) == -1)
            {
                conversationId += $"_{postConversationRequest.Participants[1]}";
            }
            else
            {
                conversationId = $"{postConversationRequest.Participants[1]}_" + conversationId; 
            }

            try
            {
                var conversation = await _conversationsService.PostConversation(postConversationRequest, conversationId);
                return StatusCode(201, conversation);
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to add conversation {conversationId} to storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while adding conversation {conversationId} to storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            try
            {
                var conversationsResponse = await _conversationsService.GetConversations(username, continuationToken, limit, lastSeenConversationTime);
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

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessageList(string conversationId, string continuationToken, int limit, long lastSeenMessageTime)
        { 
            try
            {
                var messageList = await _conversationsService.GetMessageList(conversationId, continuationToken, limit, lastSeenMessageTime);
                return Ok(messageList);
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

        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> PostMessage(string conversationId, [FromBody] PostMessageRequest postMessageRequest)
        {
            if (!ValidateMessage(postMessageRequest, out string error))
            {
                return BadRequest(error);
            }
            try
            {
                var message = await _conversationsService.PostMessage(conversationId, postMessageRequest);
                return StatusCode(201, message);
            }
            catch (ConversationNotFoundException e)
            {
                _logger.LogError(e, $"Failed to update conversation {conversationId} after adding massage {postMessageRequest.Id}");
                return StatusCode(404, $"The conversation with conversatioId { conversationId} was not found");

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
