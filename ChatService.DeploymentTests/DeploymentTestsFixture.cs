using System;
using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.IntegrationTests;


namespace Aub.Eece503e.ChatService.DeploymentTests
{
    public class DeploymentTestsFixture : IEndToEndTestsFixture
    {
            public DeploymentTestsFixture()
            {
            string serviceUrl = Environment.GetEnvironmentVariable("ChatServiceDeploymentTestsUrl");
                if (string.IsNullOrWhiteSpace(serviceUrl))
                {
                    throw new Exception("Could not find ChatServiceUrl environment variable");
                }

                ChatServiceClient = new ChatServiceClient(new System.Net.Http.HttpClient
                {
                    BaseAddress = new Uri(serviceUrl)
                });
            }
            public IChatServiceClient ChatServiceClient { get; }
    }
}
