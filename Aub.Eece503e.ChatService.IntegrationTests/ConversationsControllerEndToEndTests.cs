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

        private PostMessageRequest CreateRandomPostMessageRequest()
        {

            string id = CreateRandomString();
            string text = CreateRandomString();
            string senderUsername = CreateRandomString();
            var message = new PostMessageRequest
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

        private PostConversationRequest CreateRandomPostConversationRequest()
        {

            string[] participants = { CreateRandomString(), CreateRandomString() };
            var conversation = new PostConversationRequest
            {
                Participants = participants,
                FirstMessage = CreateRandomPostMessageRequest()
            };
            return conversation;
        }

        [Fact]
        public async Task PostGetMessage()
        {
            var conversation = CreateRandomPostConversationRequest();
            var message = CreateRandomPostMessageRequest();
            var fetchedConversation = await _chatServiceClient.AddConversation(conversation);
            var fetchedMessage = await _chatServiceClient.AddMessage(fetchedConversation.Id,message);
            Assert.Equal(message.Id, fetchedMessage.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        public async Task PostGetMessageListAssertLimitTest(int paginationLimit)
        {
            PostMessageRequest[] messageArray = new PostMessageRequest[10];
            var conversation = CreateRandomPostConversationRequest();
            var fetchedConversation = await _chatServiceClient.AddConversation(conversation);

            for (int index = 0; index < 10; index++)
            {
                messageArray[index] = CreateRandomPostMessageRequest();
            }

            for(int index = 0; index < 10; index++)
            {
                await _chatServiceClient.AddMessage(fetchedConversation.Id, messageArray[index]);
            }


            GetMessagesResponse fetchedMessageList = await _chatServiceClient.GetMessageList(fetchedConversation.Id, paginationLimit, 0);
            int countMessagesInFetchedList = fetchedMessageList.Messages.Length;

            Assert.Equal(paginationLimit, countMessagesInFetchedList);
        }

        [Fact]
        public async Task PostGetMessageListContinuationTokenTest()
        {
            var conversation = CreateRandomPostConversationRequest();
            var fetchedConversation = await _chatServiceClient.AddConversation(conversation);
            PostMessageResponse[] sentMessageList = new PostMessageResponse[6];

            for (int messageCount = 0; messageCount < 6; messageCount++)
            {
                sentMessageList[messageCount] = await _chatServiceClient.AddMessage(fetchedConversation.Id, CreateRandomPostMessageRequest());
            }

            GetMessagesResponse fetchedMessageList1 = await _chatServiceClient.GetMessageList(fetchedConversation.Id, 3, sentMessageList[0].UnixTime);
            Assert.Equal(3, fetchedMessageList1.Messages.Count());
            Assert.Equal(fetchedMessageList1.Messages.ElementAt(0).Text, sentMessageList[5].Text);
            Assert.Equal(fetchedMessageList1.Messages.ElementAt(1).Text, sentMessageList[4].Text);
            Assert.Equal(fetchedMessageList1.Messages.ElementAt(2).Text, sentMessageList[3].Text);
            Assert.NotEmpty(fetchedMessageList1.NextUri);

            GetMessagesResponse fetchedMessageList2 = await _chatServiceClient.GetMessageList(fetchedMessageList1.NextUri);
            Assert.Equal(2, fetchedMessageList2.Messages.Count());
            Assert.Equal(fetchedMessageList2.Messages.ElementAt(0).Text, sentMessageList[2].Text);
            Assert.Equal(fetchedMessageList2.Messages.ElementAt(1).Text, sentMessageList[1].Text);
            Assert.Empty(fetchedMessageList2.NextUri);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(10)]
        public async Task PostGetMessageListLastSeenMessageTimeTest(int indexOfLastSeenMessage)
        {
            var conversation = CreateRandomPostConversationRequest();
            var fetchedConversation = await _chatServiceClient.AddConversation(conversation);
            PostMessageResponse[] sentMessageList = new PostMessageResponse[11];

            for(int messageCount = 0; messageCount<11; messageCount++)
            {
                sentMessageList[messageCount] = await _chatServiceClient.AddMessage(fetchedConversation.Id, CreateRandomPostMessageRequest());
            }
           
            GetMessagesResponse fetchedMessageList = await _chatServiceClient.GetMessageList(fetchedConversation.Id, 30, sentMessageList[indexOfLastSeenMessage].UnixTime);
            int numberOfMessagesfetched = 10 - indexOfLastSeenMessage;
            Assert.Equal(numberOfMessagesfetched, fetchedMessageList.Messages.Count());
            Assert.Empty(fetchedMessageList.NextUri);
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
            var conversation = CreateRandomPostConversationRequest();
            var fetchedConversation = await _chatServiceClient.AddConversation(conversation);
            var message = new PostMessageRequest
            {
                Id = id,
                Text = text,
                SenderUsername = senderUsername
            };
            var e = await Assert.ThrowsAsync<ConversationServiceException>(() => _chatServiceClient.AddMessage(fetchedConversation.Id, message));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Fact]
        public async Task AddMessageThatAlreadyExists()
        {
            var conversation = CreateRandomPostConversationRequest();
            var message = CreateRandomPostMessageRequest();
            var fetchedConversation = await _chatServiceClient.AddConversation(conversation);
            var fetchedMessage1 = await _chatServiceClient.AddMessage(fetchedConversation.Id, message);
            var fetchedMessage2 = await _chatServiceClient.AddMessage(fetchedConversation.Id, message);
            Assert.Equal(fetchedMessage1.Id, fetchedMessage2.Id);
        }

        [Fact]
        public async Task PostGetConversation()
        {
            var conversation = CreateRandomPostConversationRequest();
            var fetchedConversation = await _chatServiceClient.AddConversation(conversation);
            Assert.Equal(fetchedConversation.Id, ParticipantsToId(conversation.Participants));
        }

        private string ParticipantsToId(string[] participants)
        {
            if(String.Compare(participants[0], participants[1]) == -1)
            {
                return $"{participants[0]}_{participants[1]}";
            }
            return $"{participants[1]}_{participants[0]}";
        }
    }
}
