using System;

namespace photo_api.Models
{
    public class ImageSummary
    {
        public string Id { get; set; }
        public Uri Thumbnail { get; set; }
        public Uri FullImage { get; set; }
        public string Caption { get; set; }
    }
}