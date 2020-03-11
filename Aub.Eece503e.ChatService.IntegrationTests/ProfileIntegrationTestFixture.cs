using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Web;
using Microsoft.AspNetCore.TestHost;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public class ProfileIntegrationTestFixture
    {
        public ProfileIntegrationTestFixture()
        {
            TestServer testServer = new TestServer(Program.CreateWebHostBuilder(new string[] { }));
            var httpClient = testServer.CreateClient();
            ProfileServiceClient = new ChatServiceClient(httpClient);
        }

        public IChatServiceClient ProfileServiceClient { get; }
    }
}
