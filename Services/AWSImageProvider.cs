using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using photo_api.Models;
using System.Linq;
using System.IO;
using Amazon.S3.Model;
using System;

namespace photo_api.Services
{
    public class AWSImageProvider : IImageProvider
    {
        private IAmazonS3 _s3client;
        private BucketOptions _options;
        private ILogger _log;

        public AWSImageProvider(BucketOptions options, IAmazonS3 s3Client, ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger(this.GetType().Name);
            _options = options;
            _s3client = s3Client;
        }

        public async Task<byte[]> GetImage(ImageType imageType, string id)
        {
            using(var stream = await GetImageStream(imageType, id))
            {
                return stream.ToArray();                
            }
        }

        public async Task<IEnumerable<ImageSummary>> GetImageSummaries()
        {
            try
            {
                var items = await _s3client.ListObjectsAsync(_options.FullImage);

                return items.S3Objects.Select(s => 
                    new ImageSummary 
                    { 
                        Caption = s.Key,
                        Id = s.Key,
                    });
            }
            catch(AmazonS3Exception ex)
            {
                _log.LogError(ex, $"GetImageSummaries");
                throw;          
            }
        }

        public Task<ImageSummary> PutImage(byte[] fileContent, string fileName, string contentType, string folder)
        {
            throw new NotImplementedException();
        }

        public async Task ReIndex()
        {
            foreach(var item in await GetImageSummaries())
            {                
                //request.Metadata.Add("x-amz-meta-title", "someTitle");
                try
                {
                    using(var outStream = new MemoryStream())
                    using(var imageStream = await GetImageStream(ImageType.FullImage, item.Id))
                    {
                        var request = new PutObjectRequest
                        {
                            BucketName = _options.ThumbNail,
                            Key = item.Id,
                            ContentType = "image/jpeg",
                            InputStream = outStream
                        };

                        ImageTransform.WriteThumbnail(imageStream, request.InputStream);
                        PutObjectResponse response = await _s3client.PutObjectAsync(request);
                    }
                }
                catch(Exception ex)
                {
                    _log.LogError(ex, $"Error building thumbnail for item {item.Id}");
                }
            }
        }

        private async Task<MemoryStream> GetImageStream(ImageType imageType, string id)
        {
            var s3Object = await _s3client.GetObjectAsync(_options.GetBucket(imageType), id);
            var stream = new MemoryStream();
            await s3Object.ResponseStream.CopyToAsync(stream);
            stream.Position = 0;
            return stream;                
        }
    }
}