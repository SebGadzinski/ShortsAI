namespace MediaCreatorSite.DataAccess.DTO
{
    public class CreditPurchaseHistory
    {
        public int id { get; set; }
        public int credit_id { get; set; }
        public Guid user_id { get; set; }
        public double amount { get; set; }
        public DateTime created_date { get; set; }
        public DateTime modified_date { get; set; }
        public string modified_by { get; set; }
    }
}
