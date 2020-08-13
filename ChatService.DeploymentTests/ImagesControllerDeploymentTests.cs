using Aub.Eece503e.ChatService.IntegrationTests;

namespace Aub.Eece503e.ChatService.DeploymentTests
{
    public class ImagesControllerDeploymentTests : ImagesControllerEndToEndTests<DeploymentTestsFixture>
    {
        public ImagesControllerDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}