namespace IOMSYS.Models
{
    public class ExpenseModel
    {
        public int ExpensesId { get; set; }
        public string ExpensesName { get; set; }
        public decimal ExpensesAmount { get; set; }
        public string? Notes { get; set; }
        public int PaymentMethodId { get; set; } 
        public int BranchId { get; set; } 
        public int UserId { get; set; } 
        public DateTime PurchaseDate { get; set; }
    }
}
