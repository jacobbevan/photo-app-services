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
        IEnumerable<ImageSummary> GetImageSummaries();
    }
}