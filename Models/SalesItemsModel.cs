namespace IOMSYS.Models
{
    public class SalesItemsModel
    {
        public int SalesInvoiceId { get; set; }
        public int SalesItemId { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public int Quantity { get; set; }
        public decimal SellPrice { get; set; }
        public decimal ItemDiscount { get; set; }
        public int? DiscountId { get; set; }
        public int? BranchId { get; set; }
        //for joins
        public string? ProductName { get; set; }
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
    }
}
