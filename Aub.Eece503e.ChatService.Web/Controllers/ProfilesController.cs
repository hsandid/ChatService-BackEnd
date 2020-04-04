using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileStore _profileStore;
        private readonly ILogger<ProfilesController> _logger;
        private readonly TelemetryClient _telemetryClient;

        public ProfilesController(IProfileStore profileStore, ILogger<ProfilesController> logger, TelemetryClient telemetryClient)
        {
            _profileStore = profileStore;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> Get(string username)
        {
            using (_logger.BeginScope("{ProfileUsername}", username))
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    Profile profile = await _profileStore.GetProfile(username);
                    _telemetryClient.TrackMetric("ProfileStore.GetProfile.Time", stopWatch.ElapsedMilliseconds);
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


        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Profile profile)
        {
            using (_logger.BeginScope("{ProfileUsername}", profile.Username))
            {
                if (!ValidateProfile(profile, out string error))
                {
                    return BadRequest(error);
                }

                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    await _profileStore.AddProfile(profile);
                    _telemetryClient.TrackMetric("ProfileStore.AddProfile.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("ProfileAdded");
                    return CreatedAtAction(nameof(Get), new { username = profile.Username }, profile);
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
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> Put(string username, [FromBody] UpdateProfileRequestBody updateProfileRequestBody)
        {
            using (_logger.BeginScope("{ProfileUsername}", username))
            {
                try
                {
                    var profile = new Profile
                    {
                        Username = username,
                        Firstname = updateProfileRequestBody.Firstname,
                        Lastname = updateProfileRequestBody.Lastname,
                        ProfilePictureId = updateProfileRequestBody.ProfilePictureId
                    };

                    if (!ValidateProfile(profile, out string error))
                    {
                        _logger.LogWarning(error);
                        return BadRequest(error);
                    }

                    var stopWatch = Stopwatch.StartNew();
                    await _profileStore.UpdateProfile(profile);
                    _telemetryClient.TrackMetric("ProfileStore.UpdateProfile.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("ProfileUpdated");
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
                catch (StorageConflictException e)
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
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> Delete(string username)
        {
            using (_logger.BeginScope("{ProfileUsername}", username))
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest("The username must not be empty or null");
                }
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    await _profileStore.DeleteProfile(username);
                    _telemetryClient.TrackMetric("ProfileStore.DeleteProfile.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("ProfileDeleted");
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