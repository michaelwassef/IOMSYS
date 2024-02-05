namespace IOMSYS.Models
{
    public class ProductsModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public byte ProductPhoto { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public decimal SellPrice { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal MaxDiscount { get; set; }
        public bool DiscountsOn { get; set; }
        public string Notes { get; set; }
    }
}
