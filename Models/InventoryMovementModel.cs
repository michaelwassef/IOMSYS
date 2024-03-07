namespace IOMSYS.Models
{
    public class InventoryMovementModel
    {
        public int MovementId { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public int Quantity { get; set; }
        public int FromBranchId { get; set; }
        public int ToBranchId { get; set; }
        public bool IsApproved { get; set; }
        public string? Notes { get; set; }
        public DateTime MovementDate { get; set; }
        public int SalesInvoiceId { get; set; }
        public int PurchaseInvoiceId { get; set; }
        //for join 
        public string? ProductName { get; set; }
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public string? FromBranchName { get; set; }
        public string? ToBranchName { get; set; }
    }
}
