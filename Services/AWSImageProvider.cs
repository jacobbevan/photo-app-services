using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using photo_api.Models;
using System.Linq;
using System.IO;
using Amazon.S3.Model;
using System;
using photo_api.Utility;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace photo_api.Services
{
    public class AWSImageProvider : IImageProvider
    {
        private IAmazonS3 _s3client;
        private IAmazonDynamoDB _dynamoDb;
        private BucketOptions _options;
        private ILogger _log;

        public AWSImageProvider(BucketOptions options, IAmazonS3 s3Client, IAmazonDynamoDB dynamoDB, ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger(this.GetType().Name);
            _options = options;
            _s3client = s3Client;
            _dynamoDb = dynamoDB;
        }

        public async Task<byte[]> GetImage(ImageType imageType, string id)
        {
            using(var stream = await GetImageStream(imageType, id))
            {
                return stream.ToArray();                
            }
        }

        public async Task<IEnumerable<ImageSummary>> GetImageSummaries(FilterCriteria filter)
        {
            try
            {

                QueryRequest queryRequest = new QueryRequest
                {
                    TableName = "azb.our-photos.images.test",
                    IndexName = "Account-Created-index",
                    KeyConditionExpression = "Account = :v_account",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":v_account", new AttributeValue { S =  "azb" }}
                    },
                    ScanIndexForward = false,
                    Limit = 50,
                    //TODO wire up ExclusiveStartKey = filter.StartKey
                };

                var result = await _dynamoDb.QueryAsync(queryRequest);

                return  result.Items.Select(r=> GetImageRecord(r)).ToList();
            }
            catch(AmazonS3Exception ex)
            {
                _log.LogError(ex, $"GetImageSummaries");
                throw;          
            }
        }

        private async Task<IEnumerable<string>> BuildImageSummariesFromS3(FilterCriteria filter)
        {
            try
            {
                var items = await _s3client.ListObjectsAsync(_options.FullImage);

                return items.S3Objects.Select(s => s.Key).Take(200);
            }
            catch(AmazonS3Exception ex)
            {
                _log.LogError(ex, $"GetImageSummaries");
                throw;          
            }
        }

        public async Task<ImageSummary> PutImage(byte[] fileContent, string fileName, string contentType, string folder)
        {            
            _log.LogInformation($"PutImage {fileName} size {fileContent.Length} bytes, contentType {contentType}, folder {folder}");

            using(var stream = new MemoryStream(fileContent))
            {
                var summary = ImageTransform.CreateImageSummary(fileName, stream);
                
                var request = new PutObjectRequest
                {
                    BucketName = _options.FullImage,
                    Key = summary.Id,
                    ContentType = contentType,
                    InputStream = stream
                };

                _log.LogInformation("begin put image");
                PutObjectResponse response = await _s3client.PutObjectAsync(request);
                _log.LogInformation("put image completes");

                //TODO can probably reuse stream
                using(var outStream = new MemoryStream(fileContent))
                {
                    await PutThumbnail(summary, outStream);
                }
                return summary;
            }


        }

        public async Task ReIndex()
        {
            foreach(var item in await BuildImageSummariesFromS3(new FilterCriteria()))
            {                
                try
                {
                    using (var imageStream = await GetImageStream(ImageType.FullImage, item))
                    {
                        var summary = ImageTransform.CreateImageSummary(item, imageStream);
                        imageStream.Position = 0;
                        await PutThumbnail(summary, imageStream);
                    }
                }
                catch(Exception ex)
                {
                    _log.LogError(ex, $"Error building thumbnail for item {item}");
                }
            }
        }
        private Task PutImageRecord(ImageSummary imageSummary)
        {            
            //TODO need to handle account properly
            return _dynamoDb.PutItemAsync(
                tableName: "azb.our-photos.images.test",
                item: new Dictionary<string, AttributeValue>
                {
                    {"Account", new AttributeValue {S = "azb"}},
                    {"Id", new AttributeValue {S = imageSummary.Id}},
                    {"Caption", new AttributeValue {S = imageSummary.Caption}},
                    {"Created", new AttributeValue {N = ToUnixFormat(imageSummary.Created).ToString()}},
                    {"Updated", new AttributeValue {N = ToUnixFormat(imageSummary.Updated).ToString()}},
                }
            );            
        }

        private ImageSummary GetImageRecord(Dictionary<string,AttributeValue> data)
        {
            //TODO need to handle account properly
            return new ImageSummary
            {
                Id = data["Id"].S,
                Caption = data["Caption"].S,
                Created  = FromUnixFormat(Convert.ToInt64(data["Created"].N)),
                Updated = FromUnixFormat(Convert.ToInt64(data["Updated"].N)),                
            };
        }


        private Task PutAlbumRecord(AlbumSummary albumSummary)
        { 
        
            var attributes = new Dictionary<string, AttributeValue>
                {
                    {"Account", new AttributeValue {S = "azb"}},
                    {"Id", new AttributeValue {S = albumSummary.Id}},
                    {"Name", new AttributeValue {S = albumSummary.Name}},
                    {"ImageIds", new AttributeValue {SS = albumSummary.ImageIds}},
                    {"Created", new AttributeValue {N = ToUnixFormat(albumSummary.Created).ToString()}},
                    {"Updated", new AttributeValue {N = ToUnixFormat(albumSummary.Updated).ToString()}},
                };

            if(!string.IsNullOrEmpty(albumSummary.Description))
            {
                 attributes["Description"] = new AttributeValue {S = albumSummary.Description};
            }

            //TODO need to handle account properly
            return _dynamoDb.PutItemAsync(
                tableName: "azb.our-photos.albums.test",
                item: attributes
            );            
        }

        private AlbumSummary GetAlbumRecord(Dictionary<string,AttributeValue> data)
        {
            //TODO need to handle account properly
            return new AlbumSummary
            {
                Id = data["Id"].S,
                Name = data["Name"].S,
                Description = data.ContainsKey("Description") ? data["Description"].S : string.Empty,
                ImageIds = data["ImageIds"].SS,
                Created  = FromUnixFormat(Convert.ToInt64(data["Created"].N)),
                Updated = FromUnixFormat(Convert.ToInt64(data["Updated"].N)),                
            };
        }

        private async Task PutThumbnail(ImageSummary item, MemoryStream imageStream)
        {
            _log.LogInformation($"PutThumbnail {item.Id}");
            using (var outStream = new MemoryStream())
            {
                _log.LogInformation($"Create thumbnail");
                ImageTransform.WriteThumbnail(imageStream, outStream);

                var request = new PutObjectRequest
                {
                    BucketName = _options.ThumbNail,
                    Key = item.Id,
                    ContentType = "image/jpeg",
                    InputStream = outStream
                };

                _log.LogInformation($"Begin S3 Put operation");
                PutObjectResponse response = await _s3client.PutObjectAsync(request);
                _log.LogInformation($"S3 Put operation completes");
            }
            await PutImageRecord(item);
        }

        private async Task<MemoryStream> GetImageStream(ImageType imageType, string id)
        {
            var s3Object = await _s3client.GetObjectAsync(_options.GetBucket(imageType), id);
            var stream = new MemoryStream();
            await s3Object.ResponseStream.CopyToAsync(stream);
            stream.Position = 0;
            return stream;                
        }

        public async Task<IEnumerable<AlbumSummary>> GetAlbumSummaries()
        {
            try
            {

                QueryRequest queryRequest = new QueryRequest
                {
                    TableName = "azb.our-photos.albums.test",
                    IndexName = "Account-Created-index",
                    KeyConditionExpression = "Account = :v_account",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":v_account", new AttributeValue { S =  "azb" }}
                    },
                    ScanIndexForward = false,
                    Limit = 50,
                    //TODO wire up ExclusiveStartKey = filter.StartKey
                };

                var result = await _dynamoDb.QueryAsync(queryRequest);

                return  result.Items.Select(r=> GetAlbumRecord(r)).ToList();
            }
            catch(AmazonS3Exception ex)
            {
                _log.LogError(ex, $"GetImageSummaries");
                throw;          
            }
        }

        public async Task DeleteImage(string id)
        {
            var attributes = new Dictionary<string, AttributeValue>
            {
                {"Id", new AttributeValue {S = id}}
            };

            _log.LogInformation($"Begin delete row data");
            await _dynamoDb.DeleteItemAsync(
                tableName: "azb.our-photos.images.test",
                key: attributes
            );       
            _log.LogInformation($"End delete row data");

            _log.LogInformation($"Begin S3 delete thumbnail");
            var thumbnail = new DeleteObjectRequest
            {
                BucketName = _options.FullImage,
                Key = id
            };
            await _s3client.DeleteObjectAsync(thumbnail);
            _log.LogInformation($"End S3 S3 delete thumbnail");                 

            _log.LogInformation($"Begin S3 delete image");
            var fullImage = new DeleteObjectRequest
            {
                BucketName = _options.FullImage,
                Key = id
            };
            await _s3client.DeleteObjectAsync(fullImage);
            _log.LogInformation($"End S3 S3 delete image");                 
        }

        public Task DeleteAlbum(string id)
        {
            var attributes = new Dictionary<string, AttributeValue>
            {
                {"Id", new AttributeValue {S = id}}
            };

            return _dynamoDb.DeleteItemAsync(
                tableName: "azb.our-photos.albums.test",
                key: attributes
            );            
        }

        public async Task UpdateAlbum(AlbumSummary value)
        {
            await PutAlbumRecord(value);
        }

        public async Task<AlbumSummary> CreateAlbum(AlbumSummary value)
        {
            //TODO review this
            value.Id = Guid.NewGuid().ToString();
            await PutAlbumRecord(value);
            return value;
        }

        public async Task<AlbumSummary> GetAlbumSummary(string id)
        {

            var attributes = new Dictionary<string, AttributeValue>
            {
                {"Id", new AttributeValue {S = id}}
            };

            var data = await _dynamoDb.GetItemAsync("azb.our-photos.albums.test", attributes);

            return GetAlbumRecord(data.Item);
        }

        public Task UpdateImage(ImageSummary value)
        {
            return PutImageRecord(value);
        }
        
        public Task<List<SearchResult>> Search(string text)
        {
            throw new NotImplementedException();
        }        

        private static long ToUnixFormat(DateTime dateTime)
        {
            var dateTimeOffset = new DateTimeOffset(dateTime);
            var unixDateTime = dateTimeOffset.ToUnixTimeSeconds();
            return unixDateTime;            
        }
        
        private static DateTime FromUnixFormat(long dateTime)
        {
            var offset = DateTimeOffset.FromUnixTimeSeconds(dateTime);
            return offset.DateTime;
        }
    }
}