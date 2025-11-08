namespace eCommerce.Web.Models.VNPay
{
    public class VnPayRequestModel
    {
        public string OrderId { get; set; }
        public string OrderDescription { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}