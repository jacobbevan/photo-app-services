using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using photo_api.Models;
using System.Linq;
using System;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

using System.IO;

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
            CreateThumbnails(_imageSummaries);
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
        
        public void CreateThumbnails(IEnumerable<ImageSummary> summaries)
        {
            foreach(var item in summaries)
            {
                CreateThumbnail(item);
            }
        }
        public void CreateThumbnail(ImageSummary summary)
        {              
            using(var inStream = File.OpenRead(GetPath(ImageType.FullImage, summary.Id)))
            using(var outStream = File.OpenWrite(GetPath(ImageType.Thumbnail, summary.Id)))
            using(var image = Image.Load<Rgba32>(inStream))
            {
                var side = Math.Min(image.Height, image.Width);
                var cropRect = new Rectangle
                {
                    X = (image.Width - side)/2,
                    Y = (image.Height - side)/2,
                    Width = side,
                    Height = side
                };
                image.Mutate(x=>x.Crop(cropRect).Resize(150,150));
                image.SaveAsJpeg(outStream);
            }
        }

        public async Task<byte[]> GetImage(ImageType imageType, string id)
        {
            return await File.ReadAllBytesAsync(GetPath(imageType, id));
        }

        private static string GetPath(ImageType imageType, string id)
        {
            return $"{IMAGE_ROOT}/{imageType.ToString()}/{id}";
        }

   }
}