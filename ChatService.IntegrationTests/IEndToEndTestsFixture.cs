using Aub.Eece503e.ChatService.Client;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public interface IEndToEndTestsFixture
    {
        public IChatServiceClient ChatServiceClient { get; }
    }
}