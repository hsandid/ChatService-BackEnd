using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.Client
{
    public class ProfileServiceClient : IProfileServiceClient
    {
        private readonly HttpClient _httpClient;

        public ProfileServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Profile> GetProfile(string username)
        {
            var responseMessage = await _httpClient.GetAsync($"api/profiles/{username}");
            EnsureSuccessOrThrow(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedProfile = JsonConvert.DeserializeObject<Profile>(json);
            return fetchedProfile;
        }

        public async Task AddProfile(Profile profile)
        {
            string json = JsonConvert.SerializeObject(profile);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync("api/profiles", new StringContent(json, Encoding.UTF8,
                "application/json"));
            EnsureSuccessOrThrow(responseMessage);
        }

        public async Task UpdateProfile(string username, Profile profile)
        {
            string json = JsonConvert.SerializeObject(profile);
            HttpResponseMessage responseMessage = await _httpClient.PutAsync($"api/profiles/{username}", new StringContent(json, Encoding.UTF8,
                "application/json"));
            EnsureSuccessOrThrow(responseMessage);
        }

        public async Task DeleteProfile(string username)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/profiles/{username}");
            EnsureSuccessOrThrow(responseMessage);
        }

        private static void EnsureSuccessOrThrow(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ProfileServiceException("", responseMessage.StatusCode);
            }
        }
    }
}