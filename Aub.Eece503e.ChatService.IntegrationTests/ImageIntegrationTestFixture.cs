using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Web;
using Microsoft.AspNetCore.TestHost;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public class ImageIntegrationTestFixture
    {
        public ImageIntegrationTestFixture()
        {
            TestServer testServer = new TestServer(Program.CreateWebHostBuilder(new string[] { }));
            var httpClient = testServer.CreateClient();
            ImageServiceClient = new ImageServiceClient(httpClient);
        }

        public IImageServiceClient ImageServiceClient { get; }
    }
}
