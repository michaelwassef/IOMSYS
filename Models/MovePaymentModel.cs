namespace IOMSYS.Models
{
    public class MovePaymentModel
    {
        public int? TransactionId { get; set; }
        public int FromBranchId { get; set; }
        public int FromPaymentMethodId { get; set; }
        public int ToBranchId { get; set; }
        public int ToPaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
