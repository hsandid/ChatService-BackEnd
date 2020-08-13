using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageStore _imageStore;
        private readonly ILogger<ImagesController> _logger;
        private readonly TelemetryClient _telemetryClient;

        public ImagesController(IImageStore imageStore, ILogger<ImagesController> logger, TelemetryClient telemetryClient)
        {
            _imageStore = imageStore;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                if (stream.Length==0)
                {
                return BadRequest("The image file is corrupted or empty");
                }

                var stopWatch = Stopwatch.StartNew();
                string imageId = await _imageStore.Upload(stream.ToArray());
                using (_logger.BeginScope("{ImageId}", imageId))
                {
                    _telemetryClient.TrackMetric("ImageStore.Upload.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("ImageAdded");
                    return CreatedAtAction(nameof(DownloadImage),
                    new { Id = imageId }, new UploadImageResponse
                    {
                        ImageId = imageId
                    });
                    
                }
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed add to add image to storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while adding image to storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }
            
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadImage(string id)
        {
            using (_logger.BeginScope("{ImageId}", id))
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    byte[] bytes = await _imageStore.Download(id);
                    _telemetryClient.TrackMetric("ImageStore.Download.Time", stopWatch.ElapsedMilliseconds);
                    return new FileContentResult(bytes, "application/octet-stream");
                }
                catch (ImageNotFoundException e)
                {
                    _logger.LogError(e, $"Image {id} already exists in storage");
                    return NotFound($"The Image with imageId {id} was not found");
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed to retrieve Image {id} from storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while retrieving Image {id} from storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            using (_logger.BeginScope("{ImageId}", id))
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("The id must not be empty or null");
                }

                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    await _imageStore.Delete(id);
                    _telemetryClient.TrackMetric("ImageStore.Delete.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("ImageDeleted");
                    return Ok(id);
                }
                catch (ImageNotFoundException e)
                {
                    _logger.LogError(e, $"Image {id} already exists in storage");
                    return NotFound($"The Image with imageId {id} was not found");
                }
                catch (StorageErrorException e)
                {
                    _logger.LogWarning(e, $"Failed to retrieve Image {id} from storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while retrieving Image {id} from storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
            }
        }
    }
}