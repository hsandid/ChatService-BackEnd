using System;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Xunit;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public class ConversationsControllerIntegrationTests : ConversationsControllerEndToEndTests<IntegrationTestsFixture>
    {
        private readonly IMessageStore _messageStore;
        private readonly IConversationStore _conversationStore;
        public ConversationsControllerIntegrationTests(IntegrationTestsFixture fixture) : base(fixture)
        {
            _messageStore = fixture.MessageStore;
            _conversationStore = fixture.ConversationStore;
        }

        [Fact]
        public async Task PostConversationEdgeCaseFailedtoAddToBothPartitions()
        {
            Profile profile1 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile1);
            Profile profile2 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile2);
            
            var messageResponse = CreateRandomPostMessageResponse(profile1.Username);
            string[] participants = { profile1.Username, profile2.Username };
            string conversationId = ParticipantsToId(participants);
            await _messageStore.AddMessage(messageResponse, conversationId);
            var messageRequest = new PostMessageRequest
            {
                Id = messageResponse.Id,
                Text = messageResponse.Text,
                SenderUsername = profile1.Username
            };
            var conversationRequest = CreateRandomPostConversationRequest(messageRequest, participants);
            var fetchedConversation = await _chatServiceClient.AddConversation(conversationRequest);
            Assert.Equal(fetchedConversation.Id, conversationId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task PostConversationEdgeCaseFailedToAddToOnePartition(int participantIndex)
        {
            Profile profile1 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile1);
            Profile profile2 = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile2);

            var messageResponse = CreateRandomPostMessageResponse(profile1.Username);
            string[] participants = { profile1.Username, profile2.Username };
            string conversationId = ParticipantsToId(participants);
            await _messageStore.AddMessage(messageResponse, conversationId);
            var messageRequest = new PostMessageRequest
            {
                Id = messageResponse.Id,
                Text = messageResponse.Text,
                SenderUsername = profile1.Username
            };
            var conversationRequest = CreateRandomPostConversationRequest(messageRequest, participants);
            var conversationResponse = new PostConversationResponse
            {
                Id = conversationId,
                CreatedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            await _conversationStore.AddConversationToPartition(conversationResponse, participants, participants[participantIndex]);
            var fetchedConversation = await _chatServiceClient.AddConversation(conversationRequest);
            Assert.Equal(fetchedConversation.Id, conversationId);
        }
    }
}
