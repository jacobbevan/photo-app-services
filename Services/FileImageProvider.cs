using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using photo_api.Models;
using System.Linq;
using System;

namespace photo_api.Services
{
    public class FileImageProvider : IImageProvider
    {
        private const string IMAGE_ROOT = "Images";
        private ILogger _log;
        private IList<ImageSummary> _imageSummaries;
        public FileImageProvider(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger(this.GetType().Name);
            _imageSummaries = LoadSummaries();
        }

        private IList<ImageSummary> LoadSummaries()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo($"{IMAGE_ROOT}/{ImageType.FullImage.ToString()}");
            return dir.EnumerateFiles().Select(
                f => new ImageSummary
                {
                    Caption = f.Name,
                    Id = f.Name,
                    Thumbnail = new Uri($"{ImageType.Thumbnail}/{f.Name})", UriKind.Relative),
                    FullImage = new Uri($"{ImageType.FullImage}/{f.Name})", UriKind.Relative),
                }).ToList();
        }

        public IEnumerable<ImageSummary> GetImageSummaries()
        {
            return _imageSummaries;
        }
        
        public async Task<byte[]> GetImage(ImageType imageType, string id)
        {
            return await System.IO.File.ReadAllBytesAsync($"{IMAGE_ROOT}/{imageType.ToString()}/{id}");
        }

   }
}