using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IMessageStore
    {
        Task<MessageWithUnixTime> AddMessage(Message message, string conversationId);
        Task<MessageList> GetMessages(string conversationId, string continuationToken, int limit);
    }
}
