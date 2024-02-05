namespace IOMSYS.Models
{
    public class DiscountsModel
    {
        public int DiscountId { get; set; }
        public string DiscountName { get; set; }
        public decimal Percentage { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
