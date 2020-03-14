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
    public  abstract class ImagesControllerEndToEndTests<TFixture> : IClassFixture<TFixture>, IAsyncLifetime where TFixture : class, IEndToEndTestsFixture

    {
        private readonly IChatServiceClient _imageServiceClient;
        private readonly Random _rand = new Random();

        private readonly ConcurrentBag<UploadImageResponse> _imagesToCleanup = new ConcurrentBag<UploadImageResponse>();

        public ImagesControllerEndToEndTests(IEndToEndTestsFixture fixture)
        {
            _imageServiceClient = fixture.ChatServiceClient;
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

            var dataStream = GenerateDataStream();
            var uploadImageResponsePost = await Upload(dataStream);

            var downloadImageResponseGet = await _imageServiceClient.DownloadImage(uploadImageResponsePost.ImageId);
            Assert.Equal(dataStream.ToArray(),downloadImageResponseGet.ImageData);
        }


        [Fact]
        public async Task GetNonExistingImage()
        {
            var uploadImageResponseGet = new UploadImageResponse 
            { 
                ImageId = GenerateRandomID() 
            };
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DownloadImage(uploadImageResponseGet.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }


        [Fact]
        public async Task AddInvalidImage()
        {
            var byteArray = new byte[0];
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => Upload(new MemoryStream(byteArray)));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Fact]
        public async Task PostDeleteImage()
        {
            var dataStream = GenerateDataStream();
            var uploadImageResponsePost = await _imageServiceClient.UploadImage(dataStream);

            await _imageServiceClient.DeleteImage(uploadImageResponsePost.ImageId);

            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DownloadImage(uploadImageResponsePost.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingImage()
        {
            var uploadImageResponse = new UploadImageResponse
            {
                ImageId = GenerateRandomID()
            };
            var e = await Assert.ThrowsAsync<ImageServiceException>(() => _imageServiceClient.DeleteImage(uploadImageResponse.ImageId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        private static string GenerateRandomID()
        {
            string imageId = Guid.NewGuid().ToString();
            return imageId;
        }
        private MemoryStream GenerateDataStream()
        {
            byte[] dataStream = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            return new MemoryStream(dataStream);
        }

        private async Task<UploadImageResponse> Upload(MemoryStream dataStream)
        {
            UploadImageResponse uploadImageResponsePost = await _imageServiceClient.UploadImage(dataStream);
            _imagesToCleanup.Add(uploadImageResponsePost);
            return uploadImageResponsePost;
        }

    }
}
