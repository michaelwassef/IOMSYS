using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

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
        public async Task<decimal?> GetExpensesAmountInExpensesAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT SUM(ExpensesAmount) AS ExpensesAmount 
                FROM Expenses
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

        public async Task<IEnumerable<DailySalesAmountModel>> GetDailySalesAmountAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
            SELECT CAST(SaleDate AS DATE) AS SaleDate, SUM(TotalAmount) AS TotalAmount 
            FROM SalesInvoices
            WHERE SaleDate >= @fromDate AND SaleDate <= @toDate AND BranchId = @branchId
            GROUP BY CAST(SaleDate AS DATE)
            ORDER BY SaleDate ASC";

            using (var db = _dapperContext.CreateConnection())
            {
                // Directly query and map the results to the DailySalesAmountModel
                var results = await db.QueryAsync<DailySalesAmountModel>(sql, new { fromDate, toDate, branchId }).ConfigureAwait(false);
                return results;
            }
        }

        public async Task<IEnumerable<BestSaleModel>> GetBestSaleAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
            SELECT TOP 5
                p.ProductId,
                p.ProductName,
                SUM(si.Quantity) AS TotalSalesQuantity
            FROM
                Products p
            INNER JOIN
                SalesItems si ON p.ProductId = si.ProductId
            GROUP BY
                p.ProductId,
                p.ProductName
            ORDER BY
                SUM(si.Quantity) DESC;";

            using (var db = _dapperContext.CreateConnection())
            {
                var results = await db.QueryAsync<BestSaleModel>(sql, new { fromDate, toDate, branchId }).ConfigureAwait(false);
                return results;
            }
        }
    }
}
