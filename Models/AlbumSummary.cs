using System;
using System.Collections.Generic;

namespace photo_api.Models {
    public  class AlbumSummary 
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> ImageIds { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}