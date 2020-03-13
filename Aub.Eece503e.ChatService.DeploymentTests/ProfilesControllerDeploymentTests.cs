using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.IntegrationTests;

namespace Aub.Eece503e.ChatService.DeploymentTests
{
    public class ProfilesControllerDeploymentTests : ProfilesControllerEndToEndTests<DeploymentTestsFixture>
    {
        public ProfilesControllerDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}