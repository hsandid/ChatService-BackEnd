using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Datacontracts;
using Aub.Eece503e.ChatService.Web.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageStore _imageStore;
        public ImagesController(IImageStore imageStore)
        {
            _imageStore = imageStore;
        }
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadImage(string id)
        {
            byte[] bytes = await _imageStore.Download(id);
            return new FileContentResult(bytes, "application/octet-stream");
        }
    }
}