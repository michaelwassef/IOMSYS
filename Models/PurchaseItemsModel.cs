namespace IOMSYS.Models
{
    public class PurchaseItemsModel
    {
        public int PurchaseInvoiceId { get; set; }
        public int PurchaseItemId { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public decimal Quantity { get; set; }
        public decimal BuyPrice { get; set; }
        public string? ProductName { get; set; }
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public decimal SellPrice { get; set; }
        public string? Notes { get; set; }
        public int BranchId { get; set; }
        public int Statues { get; set; }
        public DateTime ModDate { get; set; } 
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        public int? ModUser { get; set; }
        public string? UserName { get; set; }
    }
}
