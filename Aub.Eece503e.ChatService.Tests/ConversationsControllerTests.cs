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
using Aub.Eece503e.ChatService.Web.Services;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights;
using Aub.Eece503e.ChatService.Web;

namespace Aub.Eece503e.ChatService.Tests
{
    public class ConversationsControllerTests
    {
        private static PostMessageRequest _testPostMessageRequest = new PostMessageRequest
        {
            Id = "001",
            Text = "RandomMessage",
            SenderUsername = "JohnSmith"
        };


        private static PostMessageResponse _testMessage = new PostMessageResponse
        {
            Id = "001",
            Text = "RandomMessage",
            SenderUsername = "JohnSmith",
            UnixTime = 10
        };

        private static GetConversationsResponseEntry _testConversation = new GetConversationsResponseEntry
        {
            Id = "001",
            LastModifiedUnixTime = 000001,
            Recipient = new Profile { Username = "Joe", Firstname = "Bryan", Lastname = "Davis", ProfilePictureId = "002" }
        };

        private static PostConversationRequest _testPostConversationRequest = new PostConversationRequest
        {
            Participants = _testParticipants,
            FirstMessage =_testPostMessageRequest,
    };

        private static string _testContinuationToken = "0001";
        private static int _testLimit = 10;
        private static int _testLastSeenMessageTime = 10;
        private static string[] _testParticipants = new string[2] { "hadi", "brayan" };
        private static string _testConversationId = "hadi_brayan";

        [Fact]
        public async Task GetMessageReturns503WhenStorageIsDown()
        {
            var conversationsServiceMock = new Mock<IConversationsService>();
            conversationsServiceMock.Setup(store => store.GetMessage(_testConversation.Id, _testPostMessageRequest.Id)).ThrowsAsync(new StorageErrorException());

            var loggerStub = new ConversationsControllerLoggerStub();
            var controller = new ConversationsController(conversationsServiceMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.GetMessage(_testConversation.Id, _testPostMessageRequest.Id);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task GetMessageReturns500WhenExceptionIsNotKnown()
        {
            var conversationsServiceMock = new Mock<IConversationsService>();
            conversationsServiceMock.Setup(store => store.GetMessage(_testConversation.Id, _testPostMessageRequest.Id)).ThrowsAsync(new Exception("Test Exception"));

            var loggerStub = new ConversationsControllerLoggerStub();
            var controller = new ConversationsController(conversationsServiceMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.GetMessage(_testConversation.Id, _testPostMessageRequest.Id);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task GetMessageListReturns503WhenStorageIsDown()
        {
            var conversationsServiceMock = new Mock<IConversationsService>();
            conversationsServiceMock.Setup(store => store.GetMessageList(_testConversation.Id, _testContinuationToken, _testLimit, _testLastSeenMessageTime)).ThrowsAsync(new StorageErrorException());

            var loggerStub = new ConversationsControllerLoggerStub();
            var controller = new ConversationsController(conversationsServiceMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.GetMessageList(_testConversation.Id, _testContinuationToken, _testLimit, _testLastSeenMessageTime);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task GetMessageListReturns500WhenExceptionIsNotKnown()
        {
            var conversationsServiceMock = new Mock<IConversationsService>();
            conversationsServiceMock.Setup(store => store.GetMessageList(_testConversation.Id, _testContinuationToken, _testLimit, _testLastSeenMessageTime)).ThrowsAsync(new Exception("Test Exception"));

            var loggerStub = new ConversationsControllerLoggerStub();
            var controller = new ConversationsController(conversationsServiceMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.GetMessageList(_testConversation.Id, _testContinuationToken, _testLimit, _testLastSeenMessageTime);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task PostMessageReturns503WhenStorageIsDown()
        {
            var conversationsServiceMock = new Mock<IConversationsService>();
            conversationsServiceMock.Setup(store => store.PostMessage( _testConversation.Id, _testPostMessageRequest)).ThrowsAsync(new StorageErrorException());

            var loggerStub = new ConversationsControllerLoggerStub();
            var controller = new ConversationsController(conversationsServiceMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.PostMessage(_testConversation.Id, _testPostMessageRequest);

            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task PostMessageReturns500WhenExceptionIsNotKnown()
        {
            var conversationsServiceMock = new Mock<IConversationsService>();
            conversationsServiceMock.Setup(store => store.PostMessage(_testConversation.Id, _testPostMessageRequest)).ThrowsAsync(new Exception("Test Exception"));

            var loggerStub = new ConversationsControllerLoggerStub();
            var controller = new ConversationsController(conversationsServiceMock.Object, loggerStub, new TelemetryClient());
            IActionResult result = await controller.PostMessage(_testConversation.Id, _testPostMessageRequest);

            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        
    }

    }
