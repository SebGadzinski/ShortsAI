using MediaCreatorSite.DataAccess.Dto;
using MediaCreatorSite.DataAccess.QueryModels;

namespace MediaCreatorSite.Models
{
    public class SessionInfo
    {
        public int sessionId { get; set; }
        public AppUser? user { get; set; }
        public List<SingleClaim> claims { get; set; } = new List<SingleClaim>();
        public List<SingleRole> roles { get; set; } = new List<SingleRole>();
        public string currency { get; set; } = "USD";
        public string language { get; set; } = "en-US";
        public DateTime lastLoginDate { get; set; }
    }
}
