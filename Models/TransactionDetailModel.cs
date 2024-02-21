namespace IOMSYS.Models
{
    public class TransactionDetailModel
    {
        public int TransactionId { get; set; }
        public string InvoiceType { get; set; }
        public int? InvoiceId { get; set; }
        public string InvoiceDetail { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        public int BranchId { get; set; }
        public int PaymentMethodId { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Details { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedUser { get; set; }
        public string EntityName { get; set; }
    }

}
