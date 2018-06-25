using System;

namespace photo_api.Models
{

    public class ImageSummary
    {
        public string Id { get; set; }
        public Uri Thumbnail { get; set; }
        public Uri FullImage { get; set; }
        public string Caption { get; set; }        

        public DateTime Created {get;set;}
        public DateTime Updated {get;set;}
    
        public int? Width {get;set;}
        public int? Height {get;set;}
    }
}