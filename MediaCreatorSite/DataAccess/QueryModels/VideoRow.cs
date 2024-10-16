namespace MediaCreatorSite.DataAccess.QueryModels
{
    public class VideoRow
    {
        public int id { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public string pictureStore { get; set; }
        public string voice { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public DateTime createdOn { get; set; }
    }
}
