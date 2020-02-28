using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Web;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public class IntegrationTestFixture
    {
        public IntegrationTestFixture()
        {
            TestServer testServer = new TestServer(Program.CreateWebHostBuilder(new string[] { }));
            var httpClient = testServer.CreateClient();
            ProfileServiceClient = new ProfileServiceClient(httpClient);
        }

        public IProfileServiceClient ProfileServiceClient { get; }
    }
}
