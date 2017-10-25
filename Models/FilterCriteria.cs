namespace photo_api.Models {
    public  class FilterCriteria 
    {
        public string AlbumId { get; set; }
        public string TextSearch { get; set; }

        public override string ToString()
        {
            return $"AlbumId={AlbumId} TextSearch={TextSearch}";            
        }


    }
}