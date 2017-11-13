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
    [Route(AlbumController.API_ROUTE)]
    public class AlbumController : Controller
    {
        public const string API_ROUTE = "api/albums";
        private ILogger<AlbumController> _logger;
        private IImageProvider _imageProvider;

        public AlbumController(IImageProvider imageProvider, ILoggerFactory loggerFactory, ILogger<AlbumController> logger)
        {

            _logger = logger;
            _logger.LogInformation("ImageController created");
            _imageProvider = imageProvider;
        }

        [HttpGet()]
        public Task<IEnumerable<AlbumSummary>> GetAlbumSummaries([FromQuery] FilterCriteria filter)
        {
            return _imageProvider.GetAlbumSummaries();
        }

        [HttpGet("{id}")]
        public Task<AlbumSummary> GetAlbumSummary(string id)
        {
            return _imageProvider.GetAlbumSummary(id);
        }

        [HttpDelete("{id}")]
        public Task Delete(string id)
        {
            return _imageProvider.DeleteAlbum(id);
        }

        [HttpPut("{id}")]
        public Task UpdateAlbum(int id, [FromBody] AlbumSummary value)
        {
            return _imageProvider.UpdateAlbum(value);
        }
        
        [HttpPost()]
        public Task<AlbumSummary> CreateAlbum([FromBody] AlbumSummary value)
        {
            return _imageProvider.CreateAlbum(value);
        }
        
    }
}
