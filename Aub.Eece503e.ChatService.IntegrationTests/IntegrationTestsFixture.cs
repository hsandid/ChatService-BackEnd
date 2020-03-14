using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Web;
using Microsoft.AspNetCore.TestHost;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public class IntegrationTestsFixture: IEndToEndTestsFixture
    {
        public IntegrationTestsFixture()
        {
            TestServer testServer = new TestServer(Program.CreateWebHostBuilder(new string[] { }));
            var httpClient = testServer.CreateClient();
            ChatServiceClient = new ChatServiceClient(httpClient);
        }

        public IChatServiceClient ChatServiceClient { get; }
    }
}
