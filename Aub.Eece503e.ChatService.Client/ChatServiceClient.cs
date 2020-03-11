using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Client
{
    public class ChatServiceClient : IChatServiceClient
    {
        private readonly HttpClient _httpClient;

        public ChatServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static void EnsureSuccessOrThrowImageException(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ImageServiceException("", responseMessage.StatusCode);
            }
        }

        private static void EnsureSuccessOrThrowProfileException(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ProfileServiceException("", responseMessage.StatusCode);
            }
        }

        public async Task<UploadImageResponse> UploadImage(Stream stream)
        {
            HttpContent fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "NotNeeded"
            };
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(fileStreamContent);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,$"api/images")
                {
                    Content = formData
                };

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                EnsureSuccessOrThrowImageException(response);
                string json = await response.Content.ReadAsStringAsync();
                var uploadImageId = JsonConvert.DeserializeObject<UploadImageResponse>(json);
                return uploadImageId;
            }

        }
        public async Task<DownloadImageResponse> DownloadImage(string imageId)
        {
            using (HttpResponseMessage response = await _httpClient.GetAsync($"api/images/{imageId}"))
            {
                EnsureSuccessOrThrowImageException(response);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return new DownloadImageResponse
                {
                    ImageData = bytes
                };
            }

        }
        public async Task DeleteImage(string imageId)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/images/{imageId}");
            EnsureSuccessOrThrowImageException(responseMessage);
        }

        public async Task<Profile> GetProfile(string username)
        {
            var responseMessage = await _httpClient.GetAsync($"api/profiles/{username}");
            EnsureSuccessOrThrowProfileException(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedProfile = JsonConvert.DeserializeObject<Profile>(json);
            return fetchedProfile;
        }

        public async Task AddProfile(Profile profile)
        {
            string json = JsonConvert.SerializeObject(profile);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync("api/profiles", new StringContent(json, Encoding.UTF8,
                "application/json"));
            EnsureSuccessOrThrowProfileException(responseMessage);
        }

        public async Task UpdateProfile(string username, Profile profile)
        {
            var body = new UpdateProfileRequestBody
            {
                Firstname = profile.Firstname,
                Lastname = profile.Lastname
            };
            string json = JsonConvert.SerializeObject(body);
            HttpResponseMessage responseMessage = await _httpClient.PutAsync($"api/profiles/{username}", new StringContent(json, Encoding.UTF8,
                "application/json"));
            EnsureSuccessOrThrowProfileException(responseMessage);
        }

        public async Task DeleteProfile(string username)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/profiles/{username}");
            EnsureSuccessOrThrowProfileException(responseMessage);
        }
    }
}