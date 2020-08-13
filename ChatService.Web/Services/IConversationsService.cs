using System;
using System.Collections.Generic;
using System.Linq;
using Aub.Eece503e.ChatService.Datacontracts;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public interface IConversationsService
    {
        Task<PostMessageResponse> PostMessage(string conversationId, PostMessageRequest postMessageRequest);
        Task<GetMessagesResponse> GetMessageList(string conversationId, string continuationToken, int limit, long lastSeenMessageTime);
        Task<PostConversationResponse> PostConversation(PostConversationRequest postConversationRequest,string conversationId);
        Task<GetConversationsResponse> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime);
    }
}
