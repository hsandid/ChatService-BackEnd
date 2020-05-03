using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Web;
using Aub.Eece503e.ChatService.Web.Store;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public class IntegrationTestsFixture: IEndToEndTestsFixture
    {
        public IntegrationTestsFixture()
        {
            TestServer testServer = new TestServer(Program.CreateWebHostBuilder(new string[] { }).UseEnvironment("Development"));
            var httpClient = testServer.CreateClient();
            ChatServiceClient = new ChatServiceClient(httpClient);
            MessageStore = testServer.Host.Services.GetRequiredService<IMessageStore>();
            ConversationStore = testServer.Host.Services.GetRequiredService<IConversationStore>();
        }

        public IChatServiceClient ChatServiceClient { get; }
        public IMessageStore MessageStore { get; }
        public IConversationStore ConversationStore { get; }
    }
}
