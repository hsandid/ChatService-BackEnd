using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
	public interface IConversationStore
	{
		Task<PostConversationResponse> GetConversation(string conversationId);
		Task AddConversation(PostConversationResponse conversation);
		Task<ConversationList> GetConversations(string username, string continuationToken, int limit, long lastSeenCOnversationTime);
	}
}
