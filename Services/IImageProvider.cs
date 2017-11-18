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
        Task<AlbumSummary> GetAlbumSummary(string id);
        Task<IEnumerable<ImageSummary>> GetImageSummaries(FilterCriteria filter);
        Task<IEnumerable<AlbumSummary>> GetAlbumSummaries();
        Task ReIndex();
        Task<ImageSummary> PutImage(byte[] fileContent, string fileName, string contentType, string folder);
        Task DeleteImage(string id);
        Task DeleteAlbum(string id);
        Task UpdateAlbum(AlbumSummary value);
        Task UpdateImage(ImageSummary value);
        Task<AlbumSummary> CreateAlbum(AlbumSummary value);
    }
}