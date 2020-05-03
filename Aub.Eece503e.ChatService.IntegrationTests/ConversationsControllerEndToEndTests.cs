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
    public abstract class ConversationsControllerEndToEndTests<TFixture> : IClassFixture<TFixture>, IAsyncLifetime where TFixture : class, IEndToEndTestsFixture

    {
        public readonly IChatServiceClient _chatServiceClient;

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

        public static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        public PostMessageRequest CreateRandomPostMessageRequest()
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

        public PostMessageRequest CreateRandomPostMessageRequest(string senderUsername)
        {

            string id = CreateRandomString();
            string text = CreateRandomString();
            var message = new PostMessageRequest
            {
                Id = id,
                Text = text,
                SenderUsername = senderUsername
            };
            return message;
        }

        public PostMessageResponse CreateRandomPostMessageResponse(string senderUsername)
        {

            string id = CreateRandomString();
            string text = CreateRandomString();
            var message = new PostMessageResponse
            {
                Id = id,
                Text = text,
                SenderUsername = senderUsername,
                UnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            return message;
        }


        public Profile CreateRandomProfile()
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

        public PostConversationRequest CreateRandomPostConversationRequest()
        {

            string[] participants = { CreateRandomString(), CreateRandomString() };
            var conversation = new PostConversationRequest
            {
                Participants = participants,
                FirstMessage = CreateRandomPostMessageRequest()
            };
            return conversation;
        }

        public PostConversationRequest CreateRandomPostConversationRequest(string username1, string username2)
        {

            string[] participants = { username1, username2};
            var conversation = new PostConversationRequest
            {
                Participants = participants,
                FirstMessage = CreateRandomPostMessageRequest(username1)
            };
            return conversation;
        }

        public PostConversationRequest CreateRandomPostConversationRequest(PostMessageRequest message)
        {

            string[] participants = { CreateRandomString(), CreateRandomString() };
            var conversation = new PostConversationRequest
            {
                Participants = participants,
                FirstMessage = message
            };
            return conversation;
        }

        public PostConversationRequest CreateRandomPostConversationRequest(PostMessageRequest message, string[] participants)
        {
            var conversation = new PostConversationRequest
            {
                Participants = participants,
                FirstMessage = message
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

        [Fact]
        public async Task AddAConversationThatAlreadyExists()
        {
            var conversation = CreateRandomPostConversationRequest();
            var fetchedConversation1 = await _chatServiceClient.AddConversation(conversation);
            var fetchedConversation2 = await _chatServiceClient.AddConversation(conversation);
            Assert.Equal(fetchedConversation1.Id, fetchedConversation2.Id);
        }

        [Theory]
        [InlineData(null, "Joe", "Daniels")]
        [InlineData("fMax", null, "Daniels")]
        [InlineData("fMax", "Joe", null)]
        [InlineData("", "Joe", "Daniels")]
        [InlineData("fMax", "", "Daniels")]
        [InlineData("fMax", "Joe", "")]
        public async Task PostInvalidFisrtMessageWithConversation(string id, string text, string senderUsername)
        { 
            var message = new PostMessageRequest
            {
                Id = id,
                Text = text,
                SenderUsername = senderUsername
            };
            var conversation = CreateRandomPostConversationRequest(message);
            var e = await Assert.ThrowsAsync<ConversationServiceException>(() => _chatServiceClient.AddConversation(conversation));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        public async Task PostGetConversationListAssertLimitTest(int paginationLimit)
        {
            Profile profile1 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile1);
            PostConversationRequest[] conversationsarray = new PostConversationRequest[10];

            for (int index = 0; index < 10; index++)
            {

                Profile profile2 = CreateRandomProfile();
                await _chatServiceClient.AddProfile(profile2);
                conversationsarray[index] = CreateRandomPostConversationRequest(profile1.Username, profile2.Username);
            }

            for (int index = 0; index < 10; index++)
            {
                await _chatServiceClient.AddConversation(conversationsarray[index]);
            }

            GetConversationsResponse fetchedConversationList = await _chatServiceClient.GetConversationList(profile1.Username, paginationLimit, 0);
            int countConversationsInFetchedList = fetchedConversationList.Conversations.Length;

            Assert.Equal(paginationLimit, countConversationsInFetchedList);
        }

        [Fact]
        public async Task PostGetConversationListContinuationTokenTest()
        {
            Profile profile1 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile1);
            PostConversationResponse[] sentConversationsArray = new PostConversationResponse[6];

            for (int index = 0; index < 6; index++)
            {

                Profile profile2 = CreateRandomProfile();
                await _chatServiceClient.AddProfile(profile2);
                sentConversationsArray[index] = await _chatServiceClient.AddConversation(CreateRandomPostConversationRequest(profile1.Username, profile2.Username));
            }

            GetConversationsResponse fetchedConversationList1 = await _chatServiceClient.GetConversationList(profile1.Username, 3, sentConversationsArray[0].CreatedUnixTime);
            Assert.Equal(3, fetchedConversationList1.Conversations.Length);
            Assert.NotEmpty(fetchedConversationList1.NextUri);

            GetConversationsResponse fetchedConversationList2 = await _chatServiceClient.GetConversationList(fetchedConversationList1.NextUri);
            Assert.Equal(2, fetchedConversationList2.Conversations.Length);
            Assert.Empty(fetchedConversationList2.NextUri);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(10)]
        public async Task PostGetConversationListLastSeenConversationTimeTest(int indexOfLastSeenConversation)
        {
            Profile profile1 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile1);
            PostConversationResponse[] sentConversationsArray = new PostConversationResponse[11];

            for (int index = 0; index < 11; index++)
            {

                Profile profile2 = CreateRandomProfile();
                await _chatServiceClient.AddProfile(profile2);
                sentConversationsArray[index] = await _chatServiceClient.AddConversation(CreateRandomPostConversationRequest(profile1.Username, profile2.Username));
            }

            GetConversationsResponse fetchedConversationList = await _chatServiceClient.GetConversationList(profile1.Username, 30, sentConversationsArray[indexOfLastSeenConversation].CreatedUnixTime);
            int numberOfMessagesfetched = 10 - indexOfLastSeenConversation;
            Assert.Equal(numberOfMessagesfetched, fetchedConversationList.Conversations.Length);
            Assert.Empty(fetchedConversationList.NextUri);
        }

        [Fact]
        public async Task UpdateConversationTimeTest()
        {
            Profile profile1 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile1);
            Profile profile2 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile2);
            var originalConversation = await _chatServiceClient.AddConversation(CreateRandomPostConversationRequest(profile1.Username, profile2.Username));
            var message = await _chatServiceClient.AddMessage(originalConversation.Id,CreateRandomPostMessageRequest(profile1.Username));
            var conversations = await _chatServiceClient.GetConversationList(profile1.Username, 10, 0);
            Assert.Single(conversations.Conversations);
            Assert.Equal(conversations.Conversations[0].LastModifiedUnixTime, message.UnixTime);
        }

        public string ParticipantsToId(string[] participants)
        {
            if(String.Compare(participants[0], participants[1]) == -1)
            {
                return $"{participants[0]}_{participants[1]}";
            }
            return $"{participants[1]}_{participants[0]}";
        }
    }
}
