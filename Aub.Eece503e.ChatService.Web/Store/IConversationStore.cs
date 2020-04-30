using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
	public interface IConversationStore
	{
		Task AddConversation(PostConversationResponse conversation, string[] participants);
		Task<PostConversationResponse> GetConversation(string conversationId, string[] participants);
		Task<ConversationList> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime);

		//We still need to add a function to update conversations, maybe as a service ?
	}
}
