using System;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
namespace Aub.Eece503e.ChatService.Web
{
    //This interface is not yet implemented nor used.
    //I made a mistake by grouping IConversationStore and IMessageStore in one interface.
    //So instead of deleting the tasks related to conversations and the classes they used, I created this seperate interface which we will most probably use next lab.
    public interface IConversationStore
    {
        Task<CreateConverstaionResponse> CreateConversation(string usernam1, string usernam2, Message firstMessage);
        Task<ConversationListResponse> GetConversations(string username, string continuationToken, int limit);
    }
}
