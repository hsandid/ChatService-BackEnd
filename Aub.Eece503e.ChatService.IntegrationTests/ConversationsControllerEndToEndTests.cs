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
    public  abstract class ConversationsControllerEndToEndTests<TFixture> : IClassFixture<TFixture>, IAsyncLifetime where TFixture : class, IEndToEndTestsFixture

    {
        private readonly IChatServiceClient _chatServiceClient;
        private readonly Random _rand = new Random();

        private readonly ConcurrentBag<UploadImageResponse> _messagesToCleanup = new ConcurrentBag<UploadImageResponse>();

        public ConversationsControllerEndToEndTests(IEndToEndTestsFixture fixture)
        {
            _chatServiceClient = fixture.ChatServiceClient;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        private static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        private Message CreateRandomMessage()
        {

            string id = CreateRandomString();
            string text = CreateRandomString();
            string senderUsername = CreateRandomString();
            var message = new Message
            {
                Id = id,
                Text = text,
                SenderUsername = senderUsername
            };
            return message;
        }

        private Profile CreateRandomProfile()
        {
            string username = CreateRandomString();
            string firstname = CreateRandomString();
            var profile = new Profile
            {
                Username = username,
                Firstname = firstname,
                Lastname = "Smith"
            };
            return profile;
        }

        private Conversation CreateRandomConversation()
        {

            string id = CreateRandomString();
            var conversation = new Conversation
            {
                Id = id,
                LastModifiedUnixTime = 001,
                Recepient = CreateRandomProfile()
            };
            return conversation;
        }

        

        [Fact]
        public async Task PostGetMessage()
        {
            var message = CreateRandomMessage();
            var conversation = CreateRandomConversation();
            await _chatServiceClient.AddMessage(conversation.Id,message);

            MessageWithUnixTime fetchedMessageWithUnix = await _chatServiceClient.GetMessage(conversation.Id, message.Id);
            Message fetchedMessage = new Message
            {
                Id = fetchedMessageWithUnix.Id,
                Text = fetchedMessageWithUnix.Text,
                SenderUsername = fetchedMessageWithUnix.SenderUsername

            };

            Assert.Equal(message, fetchedMessage);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        public async Task PostGetMessageList(int paginationLimit)
        {
            Message[] messageArray = new Message[10];
            var conversation = CreateRandomConversation();

            for (int index = 0; index < 10; index++)
            {
                messageArray[index] = CreateRandomMessage();
            }

            for(int index = 0; index < 10; index++)
            {
                await _chatServiceClient.AddMessage(conversation.Id, messageArray[index]);
            }


            MessageListResponse fetchedMessageList = await _chatServiceClient.GetMessageList(conversation.Id, paginationLimit);
            int countMessagesInFetchedList = fetchedMessageList.Messages.Length;

            Assert.Equal(paginationLimit, countMessagesInFetchedList);
        }

        [Theory]
        [InlineData(null, "Joe", "Daniels")]
        [InlineData("fMax", null, "Daniels")]
        [InlineData("fMax", "Joe", null)]
        [InlineData("", "Joe", "Daniels")]
        [InlineData("fMax", "", "Daniels")]
        [InlineData("fMax", "Joe", "")]
        public async Task PostInvalidMessage(string id, string text, string senderUsername)
        {
            var conversation = CreateRandomConversation();
            var message = new Message
            {
                Id = id,
                Text = text,
                SenderUsername = senderUsername
            };
            var e = await Assert.ThrowsAsync<ConversationsServiceException>(() => _chatServiceClient.AddMessage(conversation.Id, message));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }


        [Fact]
        public async Task GetNonExistingMessage()
        {
            var message = CreateRandomMessage();
            var conversation = CreateRandomConversation();
            var e = await Assert.ThrowsAsync<ConversationsServiceException>(() => _chatServiceClient.GetMessage(conversation.Id, message.Id));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        [Fact]
        public async Task AddMessageThatAlreadyExists()
        {
            var message1 = CreateRandomMessage();
            var message2 = CreateRandomMessage();
            message2.Id = message1.Id;
            var conversation = CreateRandomConversation();
            await _chatServiceClient.AddMessage(conversation.Id, message1);
            await _chatServiceClient.AddMessage(conversation.Id, message2);

            MessageWithUnixTime fetchedMessageWithUnix = await _chatServiceClient.GetMessage(conversation.Id, message1.Id);
            Message fetchedMessage = new Message
            {
                Id = fetchedMessageWithUnix.Id,
                Text = fetchedMessageWithUnix.Text,
                SenderUsername = fetchedMessageWithUnix.SenderUsername

            };

            Assert.Equal(message1, fetchedMessage);
        }

    }
}
