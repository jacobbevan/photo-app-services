using System;

namespace photo_api.Services
{
    public class BucketOptions
    {
        public string FullImage { get; set; }
        public string ThumbNail { get; set; }

        public string GetBucket(ImageType imageType)
        {
            switch (imageType)
            {
                case ImageType.Thumbnail:
                    return ThumbNail;
                case  ImageType.FullImage:
                    return FullImage;
                default:
                    throw new NotSupportedException($"Image type {imageType.ToString()} is not supported");
            }
        }
    }
}