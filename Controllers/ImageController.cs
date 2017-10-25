using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
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
        public const string API_ROUTE = "api";
        private ILogger<ImageController> _logger;
        private IImageProvider _imageProvider;

        public ImageController(IImageProvider imageProvider, IAmazonS3 s3Client, ILoggerFactory loggerFactory, ILogger<ImageController> logger)
        {

            _logger = logger;
            _logger.LogInformation("ImageController created");
            _imageProvider = imageProvider;
        }

        [HttpGet("images")]
        public async Task<IEnumerable<ImageSummary>> GetImageSummaries()
        {
            var results = await _imageProvider.GetImageSummaries();
            return results.Select(s=>EnrichImageUris(s));
        }

        [HttpGet("albums")]
        public Task<IEnumerable<AlbumSummary>> GetAlbumSummaries([FromQuery] FilterCriteria filter)
        {
            return _imageProvider.GetAlbumSummaries(filter);
        }

        [HttpGet("images/Thumbnail/{id}")]
        public async Task<IActionResult> GetThumb(string id)
        {
            _logger.LogInformation($"GetThumb {id}");

            if(id == null)
                return this.NotFound();

            var  image = await _imageProvider.GetImage(ImageType.Thumbnail, id);

            return this.File(image, "image/jpeg");

        }

        [HttpGet("images/FullImage/{id}")]
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

        [HttpPost]
        //[Route("/api/upload")]
        public async Task<ImageSummary> Upload(IFormFile file, string folder)
        {
            if (file == null) throw new Exception("File is null");
            if (file.Length == 0) throw new Exception("File is empty");

            using (Stream stream = file.OpenReadStream())
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    var fileContent = binaryReader.ReadBytes((int)file.Length);
                    var summary = await _imageProvider.PutImage(fileContent, file.FileName, file.ContentType, folder);
                    EnrichImageUris(summary);
                    return summary;
                }
            }
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

        private static ImageSummary EnrichImageUris(ImageSummary summary)
        {
            summary.Thumbnail = new Uri($"{API_ROUTE}/images/Thumbnail/{summary.Id}", UriKind.Relative);
            summary.FullImage = new Uri($"{API_ROUTE}/images/Fullimage/{summary.Id}", UriKind.Relative);
            return summary;
        }

    }
}
