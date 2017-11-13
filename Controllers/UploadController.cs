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
    [Route(UploadController.API_ROUTE)]
    public class UploadController : Controller
    {
        public const string API_ROUTE = "api";
        private ILogger<UploadController> _logger;
        private IImageProvider _imageProvider;

        public UploadController(IImageProvider imageProvider, ILoggerFactory loggerFactory, ILogger<UploadController> logger)
        {

            _logger = logger;
            _logger.LogInformation("UploadController created");
            _imageProvider = imageProvider;
        }

        [HttpPost]
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
                    ImageController.EnrichImageUris(summary);
                    return summary;
                }
            }
        }
    }
}
