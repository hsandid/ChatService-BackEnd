using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileStore _profileStore;
        private readonly ILogger<ProfilesController> _logger;

        public ProfilesController(IProfileStore profileStore, ILogger<ProfilesController> logger)
        {
            _profileStore = profileStore;
            _logger = logger;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> Get(string username)
        {
            try
            {
                Profile profile = await _profileStore.GetProfile(username);
                return Ok(profile);
            }
            catch (ProfileNotFoundException e)
            {
                _logger.LogError(e, $"Profile {username} already exists in storage");
                return NotFound($"The profile with username {username} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to retrieve profile {username} from storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while retrieving profile {username} from storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }


        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Profile profile)
        {
            if (!ValidateProfile(profile, out string error))
            {
                return BadRequest(error);
            }

            try
            {
                await _profileStore.AddProfile(profile);
                return CreatedAtAction(nameof(Get), new { username =profile.Username },profile);
            }
            catch (ProfileAlreadyExistsException e)
            {
                _logger.LogError(e, $"Profile {profile.Username} already exists in storage");
                return Conflict($"Profile {profile.Username} already exists");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed add profile {profile} to storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while adding profile {profile} to storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> Put(string username, [FromBody] UpdateProfileRequestBody updateProfileRequestBody)
        {
            try
            {
                var profile = new Profile
                {
                    Username = username,
                    Firstname = updateProfileRequestBody.Firstname,
                    Lastname = updateProfileRequestBody.Lastname
                };

                if (!ValidateProfile(profile, out string error))
                {
                    return BadRequest(error);
                }

                await _profileStore.GetProfile(username);
                await _profileStore.UpdateProfile(profile);
                return Ok(profile);
            }
            catch (ProfileNotFoundException e)
            {
                _logger.LogError(e, $"Profile {username} does not exists in storage");
                return NotFound($"The profile with username {username} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to update profile {username} in storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch(StorageConflictException e)
            {
                _logger.LogError(e, $"Failed to update profile {username} in storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while updating profile {username} in storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> Delete(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("The username must not be empty or null");
            }
            try
            {
                await _profileStore.DeleteProfile(username);
                return Ok(username);
            }
            catch (ProfileNotFoundException e)
            {
                _logger.LogError(e, $"Profile {username} does not exists in storage");
                return NotFound($"The profile with username {username} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to delete profile {username} from storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while deleting profile {username} from storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }
        }

        private bool ValidateProfile(Profile profile, out string error)
        {
            if (string.IsNullOrWhiteSpace(profile.Username))
            {
                error = "The username must not be empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(profile.Firstname))
            {
                error = "The First Name must not be empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(profile.Lastname))
            {
                error = "The Last Name must not be empty";
                return false;
            }
            error = "";
            return true;
        }


    }
}