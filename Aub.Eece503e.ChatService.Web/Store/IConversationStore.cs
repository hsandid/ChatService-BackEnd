using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
	public interface IConversationStore
	{
		Task<ConversationClass> GetConversation(string partitionKey, string conversationId);
		Task AddConversation(ConversationClass conversation);
		Task<ConversationList> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime);
	}
}
