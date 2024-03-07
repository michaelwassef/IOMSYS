using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;

namespace IOMSYS.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly DapperContext _dapperContext;

        public DashboardService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<decimal?> GetTotalAmountInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT SUM(TotalAmount) AS TotalAmount 
                FROM PurchaseInvoices
                WHERE PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate AND BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<decimal?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
        public async Task<decimal?> GetPaidUpInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT SUM(PaidUp) AS PaidUp 
                FROM PurchaseInvoices
                WHERE PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate AND BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<decimal?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
        public async Task<decimal?> GetRemainderInPurchaseInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT SUM(Remainder) AS Remainder 
                FROM PurchaseInvoices
                WHERE PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate AND BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<decimal?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
        public async Task<decimal?> GetTotalAmountInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT SUM(TotalAmount) AS TotalAmount 
                FROM SalesInvoices
                WHERE SaleDate >= @FromDate AND SaleDate <= @ToDate AND BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<decimal?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
        public async Task<decimal?> GetPaidUpInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT SUM(PaidUp) AS PaidUp 
                FROM SalesInvoices
                WHERE SaleDate >= @FromDate AND SaleDate <= @ToDate AND BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<decimal?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
        public async Task<decimal?> GetRemainderInSalesInvoicesAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT SUM(Remainder) AS Remainder 
                FROM SalesInvoices
                WHERE SaleDate >= @FromDate AND SaleDate <= @ToDate AND BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<decimal?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
        public async Task<decimal?> CalculateProfitAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT COALESCE(SUM(CASE WHEN TransactionType = 'اضافة' THEN Amount ELSE -Amount END), 0) AS Balance
                FROM PaymentTransactions 
                WHERE BranchId = @BranchId AND TransactionDate BETWEEN @FromDate AND @ToDate";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<decimal?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
    }
}
