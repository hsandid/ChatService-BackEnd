using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
	public interface IConversationStore
	{
		Task<PostConversationResponse> AddConversation(PostConversationResponse conversation, string[] participants);
		Task<ConversationList> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime);
		Task UpdateConversation(string conversationId, long lastModifiedTime);
	}
}
