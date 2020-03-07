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
        private readonly IImageServiceClient _imageServiceClient;
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

            var downloadImage = CreateRandomDownloadImage();
            var uploadImage = await UploadImage(downloadImage);

            var downloadImageGet = await _imageServiceClient.DownloadImage(uploadImage.ImageId);
            bool isEqual = downloadImage.Equals(downloadImageGet);
            Assert.True(isEqual);
        }


            [Fact]
        public async Task GetNonExistingImage()
        {
            var uploadImage = CreateRandomUploadImage();
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DownloadImage(uploadImage.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }


        [Fact]
        public async Task AddInvalidImage()
        {
            var downloadImage = CreateRandomDownloadImage();
            downloadImage.ImageData = new byte[0];
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => UploadImage(downloadImage));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Fact]
        public async Task DeleteImage()
        {
            var downloadImage = CreateRandomDownloadImage();
            var stream = new MemoryStream(downloadImage.ImageData);
            var uploadImage = await _imageServiceClient.UploadImage(stream);

            await _imageServiceClient.DeleteImage(uploadImage.ImageId);

            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DownloadImage(uploadImage.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingImage()
        {
            var uploadImage = CreateRandomUploadImage();
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DeleteImage(uploadImage.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }
        private async Task<UploadImageResponse> UploadImage(DownloadImageResponse downloadImage)
        {
            var stream = new MemoryStream(downloadImage.ImageData);
            UploadImageResponse uploadImage = await _imageServiceClient.UploadImage(stream);
            _imagesToCleanup.Add(uploadImage);
            return uploadImage;
        }

        private static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        private UploadImageResponse CreateRandomUploadImage()
        {
            string imageId = CreateRandomString();
            var uploadImage = new UploadImageResponse
            {
                ImageId = imageId
            };
            return uploadImage;
        }
        private DownloadImageResponse CreateRandomDownloadImage()
        {
            byte[] imageData = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            var downloadImage = new DownloadImageResponse
            {
                ImageData  = imageData
            };
            return downloadImage;
        }


    }
}
