using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        string imageId = await _imageStore.Upload(stream.ToArray());
                        return CreatedAtAction(nameof(DownloadImage),
                            new { Id = imageId }, new UploadImageResponse
                            {
                                ImageId = imageId
                            });
                    }
                }
                catch (StorageErrorException e)
                {
                    _logger.LogError(e, $"Failed to upload image to storage");
                    return StatusCode(503, "The service is unavailable, please retry in few minutes");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unknown exception occured while uploading image to storage");
                    return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
                }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadImage(string id)
        {
            try
            {
                byte[] bytes = await _imageStore.Download(id);
                return new FileContentResult(bytes, "application/octet-stream");
            }
            catch (ImageNotFoundException e)
            {
                _logger.LogError(e, $"Image {id} not found");
                return NotFound($"The image with id {id} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to download image {id}");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while downloading image {id}");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            try
            {
                await _imageStore.Delete(id);
                return Ok(id);
            }
            catch (ImageNotFoundException e)
            {
                _logger.LogError(e, $"Image {id} not found");
                return NotFound($"The image with id {id} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to delete image {id}");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while deleting image {id}");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }

        }
    }
}