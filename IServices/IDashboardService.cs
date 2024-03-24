using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IDashboardService
    {
        Task<FinancialData> GetFinancialDashboardAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetTotalAmountInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetExpensesAmountInExpensesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetPaidUpInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetRemainderInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetTotalAmountInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetPaidUpInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> GetRemainderInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> CalculateProfitAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<IEnumerable<DailySalesAmountModel>> GetDailySalesAmountAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<IEnumerable<BestSaleModel>> GetBestSaleAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<FinancialData?> GetTotalItemsInPurchaseAsync(DateTime fromDate, DateTime toDate, int branchId);
        Task<decimal?> CalculateExpectedNetprofitAsync(DateTime fromDate, DateTime toDate, int branchId);
    }
}
