using System;
using System.Collections.Generic;
using System.Linq;
using Aub.Eece503e.ChatService.Datacontracts;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public interface IConversationsService
    {
        //UpdateConversation
        //GetProfileInformation
        //ADDITIONAL : Add functions from the controller to only access the store at the service layer

        Task<Profile> GetProfileInformation(string username);
        //Whenever a message is added we must update the UnixTime of a conversation!
        Task<PostMessageResponse> PostMessage(string conversationId, PostMessageRequest postMessageRequest);
        Task<PostMessageResponse> GetMessage(string conversationId, string messageId);
        Task<GetMessagesResponse> GetMessageList(string conversationId, string continuationToken, int limit, long lastSeenMessageTime);
        Task<PostConversationResponse> PostConversation(PostConversationRequest postConversationRequest,string conversationId);
        Task<PostConversationResponse> GetConversation(string conversationId, string[] participants);
        Task<GetConversationsResponse> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime);
        //Task UpdateConversation(string conversationId, string[] participants, long newModifiedUnixTime);
    }
}
