using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using photo_api.Models;
using photo_api.Services;

namespace photo_api.Controllers
{
    [Route(ImageController.API_ROUTE)]
    public class ImageController : Controller
    {
        public const string API_ROUTE = "api/images";
        private ILogger<ImageController> _logger;
        private IImageProvider _imageProvider;

        public ImageController(IImageProvider imageProvider, ILoggerFactory loggerFactory, ILogger<ImageController> logger)
        {

            _logger = logger;
            _logger.LogInformation("ImageController created");
            _imageProvider = imageProvider;
        }

        [HttpGet()]
        public async Task<IEnumerable<ImageSummary>> GetImageSummaries([FromQuery] FilterCriteria filter)
        {
            var results = await _imageProvider.GetImageSummaries(filter);
            return results.Select(s=>EnrichImageUris(s));
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


        [HttpGet("reindex")]
        public Task GetReIndex()
        {
            return _imageProvider.ReIndex();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public Task Delete(string id)
        {
            return _imageProvider.DeleteImage(id);
        }

        public static ImageSummary EnrichImageUris(ImageSummary summary)
        {
            summary.Thumbnail = new Uri($"{API_ROUTE}/Thumbnail/{summary.Id}", UriKind.Relative);
            summary.FullImage = new Uri($"{API_ROUTE}/Fullimage/{summary.Id}", UriKind.Relative);
            return summary;
        }

    }
}
