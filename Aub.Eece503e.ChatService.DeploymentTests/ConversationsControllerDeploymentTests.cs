using Aub.Eece503e.ChatService.IntegrationTests;

namespace Aub.Eece503e.ChatService.DeploymentTests
{
    public class ConversationsControllerDeploymentTests : ConversationsControllerEndToEndTests<DeploymentTestsFixture>
    {
        public ConversationsControllerDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}