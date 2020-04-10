using System;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IMessageStore
    {
        Task SendMessage(Message message, string conversationID);
        Task<CreateConverstaionResponse> CreateConversation(string usernam1, string usernam2, Message firstMessage);
        Task<MessageListResponse> GetMessages(string conversationID, string continuationToken, string limit);
        Task<ConversationListResponse> GetConversations(string username, string continuationToken, string limit);
    }
}
