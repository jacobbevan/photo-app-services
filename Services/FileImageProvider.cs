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

        public Task<IEnumerable<ImageSummary>> GetImageSummaries()
        {
            return Task.FromResult((IEnumerable<ImageSummary>)_imageSummaries);
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
            _log.LogInformation($"Create thumbnail for {summary.Id}");      
            using(var inStream = File.OpenRead(GetPath(ImageType.FullImage, summary.Id)))
            using(var outStream = File.OpenWrite(GetPath(ImageType.Thumbnail, summary.Id)))
            {
                ImageTransform.WriteThumbnail(inStream, outStream);
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

        public Task ReIndex()
        {
            CreateThumbnails(_imageSummaries);
            return Task.FromResult(true);
        }

        public async Task<ImageSummary> PutImage(byte[] fileContent, string fileName, string contentType, string folder)
        {
            var path = GetPath(ImageType.FullImage, fileName);
            await File.WriteAllBytesAsync(path, fileContent);
            
            var summary = new  ImageSummary
            {
                Caption = string.Empty,
                Id = fileName,
            };

            _imageSummaries.Add(summary);
            CreateThumbnail(summary);
            return summary;
        }
    }
}