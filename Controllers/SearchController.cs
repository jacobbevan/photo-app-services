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
    [Route(SearchController.API_ROUTE)]
    public class SearchController : Controller
    {
        public const string API_ROUTE = "api/search";
        private ILogger<UploadController> _logger;
        private IImageProvider _imageProvider;

        public SearchController(IImageProvider imageProvider, ILoggerFactory loggerFactory, ILogger<UploadController> logger)
        {
            _logger = logger;
            _imageProvider = imageProvider;
        }

        [HttpGet("{text}")]
        public Task<List<SearchResult>> Search(string text)
        {
            return _imageProvider.Search(text);
        }
    }
}
