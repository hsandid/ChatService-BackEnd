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

        private async Task EnsureSuccessOrThrowImageException(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string message = $"{responseMessage.ReasonPhrase}, {await responseMessage.Content.ReadAsStringAsync()}";
                throw new ImageServiceException(message, responseMessage.StatusCode);
            }
        }

        private async Task EnsureSuccessOrThrowProfileException(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string message = $"{responseMessage.ReasonPhrase}, {await responseMessage.Content.ReadAsStringAsync()}";
                throw new ProfileServiceException(message, responseMessage.StatusCode);
            }
        }

        private async Task EnsureSuccessOrThrowConversationsException(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string message = $"{responseMessage.ReasonPhrase}, {await responseMessage.Content.ReadAsStringAsync()}";
                throw new ChatServiceException(message, responseMessage.StatusCode);
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
                await EnsureSuccessOrThrowImageException(response);
                string json = await response.Content.ReadAsStringAsync();
                var uploadImageId = JsonConvert.DeserializeObject<UploadImageResponse>(json);
                return uploadImageId;
            }

        }
        public async Task<DownloadImageResponse> DownloadImage(string imageId)
        {
            using (HttpResponseMessage response = await _httpClient.GetAsync($"api/images/{imageId}"))
            {
               await EnsureSuccessOrThrowImageException(response);
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
            await EnsureSuccessOrThrowImageException(responseMessage);
        }

        public async Task<Profile> GetProfile(string username)
        {
            var responseMessage = await _httpClient.GetAsync($"api/profile/{username}");
            await EnsureSuccessOrThrowProfileException(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedProfile = JsonConvert.DeserializeObject<Profile>(json);
            return fetchedProfile;
        }

        public async Task AddProfile(Profile profile)
        {
            string json = JsonConvert.SerializeObject(profile);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync("api/profile", new StringContent(json, Encoding.UTF8,
                "application/json"));
            await EnsureSuccessOrThrowProfileException(responseMessage);
        }

        public async Task UpdateProfile(string username, Profile profile)
        {
            var body = new UpdateProfileRequestBody
            {
                Firstname = profile.Firstname,
                Lastname = profile.Lastname
            };
            string json = JsonConvert.SerializeObject(body);
            HttpResponseMessage responseMessage = await _httpClient.PutAsync($"api/profile/{username}", new StringContent(json, Encoding.UTF8,
                "application/json"));
            await EnsureSuccessOrThrowProfileException(responseMessage);
        }
        public async Task DeleteProfile(string username)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/profile/{username}");
            await EnsureSuccessOrThrowProfileException(responseMessage);
        }

        public async Task<Message> AddMessage(string conversationId, PostMessageRequest message)
        {
            string json = JsonConvert.SerializeObject(message);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync($"api/conversations/{conversationId}/messages", new StringContent(json, Encoding.UTF8,
                "application/json"));
            await EnsureSuccessOrThrowConversationsException(responseMessage);
            string responseJson = await responseMessage.Content.ReadAsStringAsync();
            var fetchedMessage = JsonConvert.DeserializeObject<Message>(responseJson);
            return fetchedMessage;
        }
        public async Task<Message> GetMessage(string conversationId, string messageId)
        {
            var responseMessage = await _httpClient.GetAsync($"api/conversations/{conversationId}/messages/{messageId}");
            await EnsureSuccessOrThrowConversationsException(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedMessage = JsonConvert.DeserializeObject<Message>(json);
            return fetchedMessage;
        }
        public async Task<GetMessagesResponse> GetMessageList(string conversationId, int limit, long lastSeenMessageTime)
        {
            var responseMessage = await _httpClient.GetAsync($"api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}");
            await EnsureSuccessOrThrowConversationsException(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedMessageList = JsonConvert.DeserializeObject<GetMessagesResponse>(json);
            return fetchedMessageList;
        }

        public async Task<GetMessagesResponse> GetMessageList(string conversationId, string uri)
        {
            var responseMessage = await _httpClient.GetAsync(uri);
            await EnsureSuccessOrThrowConversationsException(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedMessageList = JsonConvert.DeserializeObject<GetMessagesResponse>(json);
            return fetchedMessageList;
        }
    }
}