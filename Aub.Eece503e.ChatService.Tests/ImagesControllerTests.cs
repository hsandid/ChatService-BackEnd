using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Controllers;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Aub.Eece503e.ChatService.Tests
{
    public class ImagesControllerTests
    {
        private DownloadImageResponse _testImageDowload = new DownloadImageResponse
        {
            ImageData = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())
        };

        private UploadImageResponse _testImageUpload = new UploadImageResponse
        {
            ImageId = Guid.NewGuid().ToString()
        };

        [Fact]
        public async Task UploadImageReturns503WhenStorageIsDown()
        {
            var stream = new MemoryStream(_testImageDowload.ImageData);
            IFormFile file = new FormFile(stream, 0, _testImageDowload.ImageData.Length, "file", "fileName");
            var imageStoreMock = new Mock<IImageStore>();
            
            imageStoreMock.Setup(store => store.Upload(_testImageDowload.ImageData)).ThrowsAsync(new StorageErrorException());
            
            var loggerStub = new ImagesControllerLoggerStub();
            var controller = new ImagesController(imageStoreMock.Object, loggerStub);
            IActionResult result = await controller.UploadImage(file);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task UploadImageReturns500WhenExceptionIsNotKnown()
        {
            var stream = new MemoryStream(_testImageDowload.ImageData);
            IFormFile file = new FormFile(stream, 0, _testImageDowload.ImageData.Length, "file", "fileName");
            var imageStoreMock = new Mock<IImageStore>();
            imageStoreMock.Setup(store => store.Upload(_testImageDowload.ImageData)).ThrowsAsync(new Exception("Test Exception"));
            
            var loggerStub = new ImagesControllerLoggerStub();
            var controller = new ImagesController(imageStoreMock.Object, loggerStub);
            IActionResult result = await controller.UploadImage(file);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task DownloadImageReturns503WhenStorageIsDown()
        {
            var imageStoreMock = new Mock<IImageStore>();
            imageStoreMock.Setup(store => store.Download(_testImageUpload.ImageId)).ThrowsAsync(new StorageErrorException());
            
            var loggerStub = new ImagesControllerLoggerStub();
            var controller = new ImagesController(imageStoreMock.Object, loggerStub);
            IActionResult result = await controller.DownloadImage(_testImageUpload.ImageId);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task DownloadImageReturns500WhenExceptionIsNotKnown()
        {
            var imageStoreMock = new Mock<IImageStore>();
            imageStoreMock.Setup(store => store.Download(_testImageUpload.ImageId)).ThrowsAsync(new Exception("Test Exception"));
            
            var loggerStub = new ImagesControllerLoggerStub();
            var controller = new ImagesController(imageStoreMock.Object, loggerStub);
            IActionResult result = await controller.DownloadImage(_testImageUpload.ImageId);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task DeleteImageReturns503WhenStorageIsDown()
        {
            var imageStoreMock = new Mock<IImageStore>();
            imageStoreMock.Setup(store => store.Delete(_testImageUpload.ImageId)).ThrowsAsync(new StorageErrorException());
            
            var loggerStub = new ImagesControllerLoggerStub();
            var controller = new ImagesController(imageStoreMock.Object, loggerStub);
            IActionResult result = await controller.DeleteImage(_testImageUpload.ImageId);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task DeleteImageReturns500WhenExceptionIsNotKnown()
        {
            var imageStoreMock = new Mock<IImageStore>();
            imageStoreMock.Setup(store => store.Delete(_testImageUpload.ImageId)).ThrowsAsync(new Exception("Test Exception"));
            
            var loggerStub = new ImagesControllerLoggerStub();
            var controller = new ImagesController(imageStoreMock.Object, loggerStub);
            IActionResult result = await controller.DeleteImage(_testImageUpload.ImageId);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

    }


}
