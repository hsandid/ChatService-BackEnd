using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageStore _imageStore;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IImageStore imageStore, ILogger<ImagesController> logger)
        {
            _imageStore = imageStore;
            _logger = logger;
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
                string imageId = await _imageStore.Upload(stream.ToArray());
                return CreatedAtAction(nameof(DownloadImage),
                new { Id = imageId }, new UploadImageResponse
                {
                    ImageId = imageId
                });
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

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("The id must not be empty or null");
            }

            try
            {
                byte[] bytes = await _imageStore.Download(id);
                return new FileContentResult(bytes, "application/octet-stream");
            }
            catch (BlobNotFoundException e)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("The id must not be empty or null");
            }

            try
            {
                await _imageStore.Delete(id);
                return Ok(id);
            }
            catch (BlobNotFoundException e)
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
}