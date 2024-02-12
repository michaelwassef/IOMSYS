namespace IOMSYS.Models
{
    public class ProductsModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public byte[]? ProductPhoto { get; set; }
        public int ProductTypeId { get; set; }
        public string? ProductTypeName { get; set; }
        public decimal SellPrice { get; set; }
        public decimal BuyPrice { get; set; }
        public int MinQuantity { get; set; }
        public decimal MaxDiscount { get; set; }
        public bool DiscountsOn { get; set; }
        public string? Notes { get; set; }

        //for joins
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public int? TotalQuantity { get; set; }
        public decimal? TotalBuyPrice { get; set; }
        public decimal? TotalSellPrice { get; set; }
        public int? TotalSoldQuantity { get; set; }
        public int? AvailableQty { get; set; }
    }
}
