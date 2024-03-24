namespace IOMSYS.Models
{
    public class FinancialData
    {
        public decimal? TotalAmountInPurchaseInvoices { get; set; }
        public decimal? PaidUpInPurchaseInvoices { get; set; }
        public decimal? RemainderInPurchaseInvoices { get; set; }
        public decimal? TotalAmountInSalesInvoices { get; set; }
        public decimal? PaidUpInSalesInvoices { get; set; }
        public decimal? RemainderInSalesInvoices { get; set; }
        public decimal? ExpensesAmount { get; set; }
        public decimal? Profit { get; set; }
        public decimal? ExpectedNet { get; set; }

        public decimal? TotalBuyCost { get; set;}
        public decimal? TotalSellRevenue { get; set;}
    }
}
