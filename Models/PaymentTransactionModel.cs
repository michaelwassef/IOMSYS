namespace IOMSYS.Models
{
    public class PaymentTransactionModel
    {
        public int? TransactionId { get; set; }
        public int BranchId { get; set; }
        public int PaymentMethodId { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string? Details { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedUser { get; set; }
        public int? InvoiceId { get; set; }
    }
}
