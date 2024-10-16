namespace MediaCreatorSite.Utility.Results
{
    public class EditResult : BaseResult
    {
        public Dictionary<string, string> ErrorAttributes { get; set; } = new Dictionary<string, string>();
    }
}
