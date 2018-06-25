using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using photo_api.Models;
using System.Linq;
using System;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System.IO;
using photo_api.Utility;
using System.Text.RegularExpressions;

namespace photo_api.Services
{
    public class FileImageProvider : IImageProvider
    {
        private const string IMAGE_ROOT = "Images";
        private ILogger _log;
        private IList<ImageSummary> _imageSummaries;
        private IList<AlbumSummary> _albumSummaries;
        public FileImageProvider(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger(this.GetType().Name);
            _imageSummaries = LoadSummaries();
            _albumSummaries = CreateSampleAlbum(_imageSummaries).ToList();
        }

        private IEnumerable<AlbumSummary> CreateSampleAlbum(IList<ImageSummary> imageSummaries)
        {
            yield return new AlbumSummary 
            {
                Created = DateTime.Now,
                Updated = DateTime.Now,
                Description = "This is a sample album",
                Id = "album1",
                ImageIds = imageSummaries.Select(i=>i.Id).Take(3).ToList(),
                Name = "Sample Album"
            };
        }

        private IList<ImageSummary> LoadSummaries()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo($"{IMAGE_ROOT}/{ImageType.FullImage.ToString()}");
            return dir.EnumerateFiles().Select(
                f => new ImageSummary
                {
                    Caption = f.Name,
                    Id = f.Name,
                }).ToList();
        }

        public async Task<IEnumerable<ImageSummary>> GetImageSummaries(FilterCriteria filter)
        {
            if(string.IsNullOrEmpty(filter.AlbumId))
            {
                return (IEnumerable<ImageSummary>)_imageSummaries;
            }

            var album = await GetAlbumSummary(filter.AlbumId);
            return album.ImageIds.Select(i=>_imageSummaries.FirstOrDefault(j=>j.Id == i)).Where(k=>k != null).ToList();
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

        public Task<IEnumerable<AlbumSummary>> GetAlbumSummaries()
        {
            return Task.FromResult(_albumSummaries as IEnumerable<AlbumSummary>);
        }

        public Task DeleteImage(string id)
        {
            _imageSummaries = _imageSummaries.Where(i=>i.Id != id).ToList();
            foreach(var album in _albumSummaries)
            {
                album.ImageIds = album.ImageIds.Where(i=> i != id).ToList();
            }
            return Task.FromResult(true);
        }

        public Task DeleteAlbum(string id)
        {
            _albumSummaries = _albumSummaries.Where(i=>i.Id != id).ToList();
            return Task.FromResult(true);
        }

        public Task UpdateAlbum(AlbumSummary value)
        {
            _albumSummaries = _albumSummaries.Where(a=>a.Id != value.Id).Append(value).ToList();
            return Task.FromResult(true);
        }

        public Task<AlbumSummary> CreateAlbum(AlbumSummary value)
        {
            value.Id = Guid.NewGuid().ToString();
            _albumSummaries.Add(value);
            return Task.FromResult(value);
        }

        public Task<AlbumSummary> GetAlbumSummary(string id)
        {
            return Task.FromResult(_albumSummaries.Where(a=>a.Id == id).First());
        }

        public Task UpdateImage(ImageSummary value)
        {
            _imageSummaries = _imageSummaries.Where(a=>a.Id != value.Id).Append(value).ToList();
            return Task.FromResult(true);
        }
        
        public Task<List<SearchResult>> Search(string text)
        {
            var upper = text.ToUpper();
            var matches = new List<SearchResult>();
            foreach(var album in _albumSummaries)
            {
                if(album.Name.ToUpper().Contains(upper))
                {
                    matches.Add
                    (
                        new SearchResult
                        {
                            Id = album.Id,
                            Type = "ALBUM",
                            Title = album.Name
                        }
                    );
                }
            }

            foreach(var image in _imageSummaries)
            {
                if(image.Caption.ToUpper().Contains(upper))
                {
                    matches.Add
                    (
                        new SearchResult
                        {
                            Id = image.Id,
                            Type = "IMAGE",
                            Title = image.Caption
                        }
                    );
                }
            }            
            
            return Task.FromResult(matches);
        }        
    }
}