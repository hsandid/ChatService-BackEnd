using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Datacontracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public class ImagesControllerIntegrationTests : IClassFixture<ImageIntegrationTestFixture>, IAsyncLifetime 
    {
        private readonly IChatServiceClient _imageServiceClient;
        private readonly Random _rand = new Random();

        private readonly ConcurrentBag<UploadImageResponse> _imagesToCleanup = new ConcurrentBag<UploadImageResponse>();

        public ImagesControllerIntegrationTests(ImageIntegrationTestFixture fixture)
        {
            _imageServiceClient = fixture.ImageServiceClient;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var tasks = new List<Task>();
            foreach (var image in _imagesToCleanup)
            {
                var task = _imageServiceClient.DeleteImage(image.ImageId);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task PostGetImage()
        {

            var downloadImageResponsePost = GenerateDownloadImageResponseEntity();
            var uploadImageResponsePost = await Upload(downloadImageResponsePost);

            var downloadImageResponseGet = await _imageServiceClient.DownloadImage(uploadImageResponsePost.ImageId);
            Assert.Equal(downloadImageResponsePost,downloadImageResponseGet);
        }


        [Fact]
        public async Task GetNonExistingImage()
        {
            var uploadImageResponseGet = GenerateUploadImageResponseEntity();
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DownloadImage(uploadImageResponseGet.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }


        [Fact]
        public async Task AddInvalidImage()
        {
            var downloadImageResponsePost = GenerateDownloadImageResponseEntity();
            downloadImageResponsePost.ImageData = new byte[0];
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => Upload(downloadImageResponsePost));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Fact]
        public async Task PostDeleteImage()
        {
            var downloadImageResponsePost = GenerateDownloadImageResponseEntity();
            var stream = new MemoryStream(downloadImageResponsePost.ImageData);
            var uploadImageResponsePost = await _imageServiceClient.UploadImage(stream);

            await _imageServiceClient.DeleteImage(uploadImageResponsePost.ImageId);

            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DownloadImage(uploadImageResponsePost.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingImage()
        {
            var uploadImageResponseRandom = GenerateUploadImageResponseEntity();
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DeleteImage(uploadImageResponseRandom.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        private static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        private UploadImageResponse GenerateUploadImageResponseEntity()
        {
            string imageId = CreateRandomString();
            var uploadImage = new UploadImageResponse
            {
                ImageId = imageId
            };
            return uploadImage;
        }
        private DownloadImageResponse GenerateDownloadImageResponseEntity()
        {
            byte[] imageData = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            var downloadImage = new DownloadImageResponse
            {
                ImageData  = imageData
            };
            return downloadImage;
        }

        private async Task<UploadImageResponse> Upload(DownloadImageResponse downloadImageResponsePost)
        {
            var stream = new MemoryStream(downloadImageResponsePost.ImageData);
            UploadImageResponse uploadImageResponsePost = await _imageServiceClient.UploadImage(stream);
            _imagesToCleanup.Add(uploadImageResponsePost);
            return uploadImageResponsePost;
        }

    }
}
