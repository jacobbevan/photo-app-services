using System.Collections.Generic;
using System.Threading.Tasks;
using photo_api.Models;

namespace photo_api.Services
{
    public enum ImageType
    {
        Thumbnail,
        FullImage
    }

    public interface IImageProvider
    {
        Task<byte[]> GetImage(ImageType imageType, string id);
        Task<IEnumerable<ImageSummary>> GetImageSummaries();
        Task ReIndex();
        Task<ImageSummary> PutImage(byte[] fileContent, string fileName, string contentType, string folder);
    }
}