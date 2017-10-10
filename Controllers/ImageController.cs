using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using photo_api.Models;
using photo_api.Services;

namespace photo_api.Controllers
{
    [Route("api/images")]
    public class ImageController : Controller
    {
        private ILogger<ImageController> _logger;
        private IImageProvider _imageProvider;

        public ImageController(IAmazonS3 s3Client, ILoggerFactory loggerFactory, ILogger<ImageController> logger)
        {
            _logger.LogInformation("ImageController created");
            _logger = logger;
            _imageProvider = new FileImageProvider(loggerFactory);
        }

        [HttpGet]
        public IEnumerable<ImageSummary> Get()
        {
            return _imageProvider.GetImageSummaries();
        }

        [HttpGet("Thumbnail/{id}")]
        public async Task<IActionResult> GetThumb(string id)
        {
            _logger.LogInformation($"GetThumb {id}");

            if(id == null)
                return this.NotFound();

            var  image = await _imageProvider.GetImage(ImageType.Thumbnail, id);

            return this.File(image, "image/jpeg");

        }

        [HttpGet("FullImage/{id}")]
        public async Task<IActionResult> GetFull(string id)
        {
            if(id == null)
                return this.NotFound();

            var  image = await _imageProvider.GetImage(ImageType.FullImage, id);

            return this.File(image, "image/jpeg");
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
