namespace IOMSYS.Models
{
    public class SalesInvoicesModel
    {
        public int SalesInvoiceId { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidUp { get; set; }
        public decimal Remainder { get; set; }
        public int BranchId { get; set; }
        public int PaymentMethodId { get; set; }
        public int UserId { get; set; }
        public DateTime SaleDate { get; set; }

        //for joins
        public string CustomerName { get; set; }
        public string BranchName { get; set; }
        public string PaymentMethodName { get; set; }
        public string UserName { get; set; }
    }
}
