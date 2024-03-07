namespace IOMSYS.IServices
{
    public interface IDashboardService
    {
        Task<decimal?> GetTotalAmountInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetPaidUpInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetRemainderInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetTotalAmountInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetPaidUpInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetRemainderInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> CalculateProfitAsync(DateTime fromDate, DateTime toDate, int branchId);
    }
}
