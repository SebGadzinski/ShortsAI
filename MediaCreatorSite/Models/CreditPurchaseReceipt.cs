namespace MediaCreatorSite.Models
{
    public class CreditPurchaseReceipt
    {
        public int creditPurchaseHistoryId { get; set; }
        public double creditsPucharchased { get; set; }
        public string charge { get; set; }
        public string last4DigitsOfCard { get; set; }
    }
}
