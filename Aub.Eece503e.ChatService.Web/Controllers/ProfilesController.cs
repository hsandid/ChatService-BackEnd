using Aub.Eece503e.ChatService.Web.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileStore _profileStore;
        private readonly ILogger<ProfilesController> _logger;

        /*public ProfilesController(IProfileStore profileStore, ILogger<ProfilesController> logger)
        {
            _profileStore = profileStore;
            _logger = logger;

            //If uncommented, this will throw an exception since there is no Profiles table in azure to connect to yet!!!
        }*/

        [HttpGet("test")]
        public IActionResult GetTest()
        {
            return Ok("Test is successful");
        }


    }
}