namespace IOMSYS.Models
{
    public class PurchaseInvoicesModel
    {
        public int PurchaseInvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidUp { get; set; }
        public decimal Remainder { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public int PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime PurchaseDate { get; set; }

        // New property to hold associated purchase items
        public List<PurchaseItemsModel> PurchaseItems { get; set; }
        public PurchaseInvoicesModel()
        {
            PurchaseItems = new List<PurchaseItemsModel>();
        }
    }
}
