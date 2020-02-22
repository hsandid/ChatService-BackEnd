using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public interface IProfileStore
    {
        Task AddProfile(Profile profile);
        Task<Profile> GetProfile();
        Task DeleteProfile(string username);
        Task UpdateProfile(Profile profile);
    }
}
