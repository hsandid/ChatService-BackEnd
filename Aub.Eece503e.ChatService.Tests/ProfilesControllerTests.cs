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
using Microsoft.ApplicationInsights;

namespace Aub.Eece503e.ChatService.Tests
{
    public class ProfilesControllerTests
    {
        private Profile _testProfile = new Profile
        {
            Username = "Hasaxc",
            Firstname = "John",
            Lastname = "Smith"
        };

        [Fact]
        public async Task AddProfileReturns503WhenStorageIsDown()
        {
            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.AddProfile(_testProfile)).ThrowsAsync(new StorageErrorException());
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Post(_testProfile);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task AddProfileReturns500WhenExceptionIsNotKnown()
        {
            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.AddProfile(_testProfile)).ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Post(_testProfile);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task GetProfileReturns503WhenStorageIsDown()
        {
            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.GetProfile(_testProfile.Username)).ThrowsAsync(new StorageErrorException());
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Get(_testProfile.Username);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task GetProfileReturns500WhenExceptionIsNotKnown()
        {
            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.GetProfile(_testProfile.Username)).ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Get(_testProfile.Username);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task UpdateProfileReturns503WhenStorageIsDown()
        {
            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.UpdateProfile(_testProfile)).ThrowsAsync(new StorageErrorException());
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Put(_testProfile.Username, new UpdateProfileRequestBody() { Firstname = _testProfile.Firstname, Lastname = _testProfile.Lastname });

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task UpdateProfileReturns500WhenExceptionIsNotKnown()
        {
            UpdateProfileRequestBody passedProfile = new UpdateProfileRequestBody();
            passedProfile.Firstname = _testProfile.Firstname;
            passedProfile.Lastname = _testProfile.Lastname;

            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.UpdateProfile(_testProfile)).ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Put(_testProfile.Username, passedProfile);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task UpdateProfileReturn503WhenOptimisticConcurrencyFails()
        {
            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.UpdateProfile(_testProfile)).ThrowsAsync(new StorageConflictException());
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Put(_testProfile.Username, new UpdateProfileRequestBody() { Firstname = _testProfile.Firstname, Lastname = _testProfile.Lastname });

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }
        
        [Fact]
        public async Task DeleteProfileReturns503WhenStorageIsDown()
        {
            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.DeleteProfile(_testProfile.Username)).ThrowsAsync(new StorageErrorException());
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Delete(_testProfile.Username);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task DeleteProfileReturns500WhenExceptionIsNotKnown()
        {
            var profilesStoreMock = new Mock<IProfileStore>();
            profilesStoreMock.Setup(store => store.DeleteProfile(_testProfile.Username)).ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new ProfilesControllerLoggerStub();
            var controller = new ProfileController(profilesStoreMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.Delete(_testProfile.Username);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

    }


}
