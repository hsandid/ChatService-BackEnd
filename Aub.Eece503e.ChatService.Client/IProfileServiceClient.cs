using System.Collections.Generic;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Client
{
    public interface IProfileServiceClient
    {
        Task AddProfile(Profile profile);
        Task<Profile> GetProfile(string username);
        Task DeleteProfile(string username);
        Task UpdateProfile(string username, Profile profile);
    }
}
