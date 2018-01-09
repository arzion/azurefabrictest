namespace Wirex.Engine.Api.Data
{
    public class PlaceOrderData
    {
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set; }
        public Side Side { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
    }
}
