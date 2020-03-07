using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;

namespace Aub.Eece503e.ChatService.Client
{
    public class ImageServiceClient : IImageServiceClient
    {
        private readonly HttpClient _httpClient;

        public ImageServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static void EnsureSuccessOrThrow(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ImageServiceException("", responseMessage.StatusCode);
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
                EnsureSuccessOrThrow(response);
                string json = await response.Content.ReadAsStringAsync();
                var uploadImageId = JsonConvert.DeserializeObject<UploadImageResponse>(json);
                return uploadImageId;
            }

        }
        public async Task<DownloadImageResponse> DownloadImage(string imageId)
        {
            using (HttpResponseMessage response = await _httpClient.GetAsync($"api/images/{imageId}"))
            {
                EnsureSuccessOrThrow(response);
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
            EnsureSuccessOrThrow(responseMessage);
        }    
    }
}