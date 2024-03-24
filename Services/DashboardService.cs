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

        public async Task<FinancialData> GetFinancialDashboardAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var totalItemsInPurchase = await GetTotalItemsInPurchaseAsync(fromDate, toDate, branchId) ?? new FinancialData();
            var dashboard = new FinancialData
            {
                TotalAmountInPurchaseInvoices = await GetTotalAmountInPurchaseInvoicesAsync(fromDate, toDate, branchId) ?? 0,
                PaidUpInPurchaseInvoices = await GetPaidUpInPurchaseInvoicesAsync(fromDate, toDate, branchId) ?? 0,
                RemainderInPurchaseInvoices = await GetRemainderInPurchaseInvoicesAsync(fromDate, toDate, branchId) ?? 0,
                TotalAmountInSalesInvoices = await GetTotalAmountInSalesInvoicesAsync(fromDate, toDate, branchId) ?? 0,
                PaidUpInSalesInvoices = await GetPaidUpInSalesInvoicesAsync(fromDate, toDate, branchId) ?? 0,
                RemainderInSalesInvoices = await GetRemainderInSalesInvoicesAsync(fromDate, toDate, branchId) ?? 0,
                ExpensesAmount = await GetExpensesAmountInExpensesAsync(fromDate, toDate, branchId) ?? 0,
                Profit = await CalculateProfitAsync(fromDate, toDate, branchId) ?? 0,
                ExpectedNet = await CalculateExpectedNetprofitAsync(fromDate, toDate, branchId) ?? 0,
                TotalBuyCost = totalItemsInPurchase.TotalBuyCost ?? 0,
                TotalSellRevenue = totalItemsInPurchase.TotalSellRevenue ?? 0
            };
            return dashboard;
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
                WHERE SaleDate >= @FromDate AND SaleDate <= @ToDate AND BranchId = @BranchId AND IsReturn = 0";

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
              SELECT 
                CASE 
                    WHEN NetPaidUp < 0 THEN 0 
                    ELSE NetPaidUp 
                END AS NetPaidUp
            FROM (
                SELECT 
                    COALESCE(
                        (
                            COALESCE((SELECT SUM(PaidUp)
                                      FROM SalesInvoices
                                      WHERE SaleDate >= @FromDate AND SaleDate <= @ToDate AND BranchId = @BranchId), 0)
                            -
                            (
                                COALESCE((SELECT SUM(PaidUp)
                                           FROM PurchaseInvoices
                                           WHERE PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate AND BranchId = @BranchId), 0)
                                +
                                COALESCE((SELECT SUM(ExpensesAmount)
                                           FROM Expenses
                                           WHERE PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate AND BranchId = @BranchId), 0)
                            )
                        ), 0
                    ) AS NetPaidUp
            ) AS SubQuery;";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<decimal?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
        public async Task<decimal?> CalculateExpectedNetprofitAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
        SELECT 
            CASE 
                WHEN netProfit < 0 THEN 0 
                ELSE netProfit 
            END AS ExpectedNet
        FROM (
            SELECT 
                COALESCE(
                    (
                        COALESCE((SELECT SUM(TotalAmount)
                                  FROM SalesInvoices
                                  WHERE SaleDate >= @FromDate AND SaleDate <= @ToDate AND BranchId = @BranchId), 0)
                        -
                        COALESCE((SELECT SUM(TotalAmount)
                                   FROM PurchaseInvoices
                                   WHERE PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate AND BranchId = @BranchId), 0)
                    ),
                    0
                ) AS netProfit
        ) AS SubQuery;";

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
        public async Task<FinancialData?> GetTotalItemsInPurchaseAsync(DateTime fromDate, DateTime toDate, int branchId)
        {
            var sql = @"
                SELECT 
                    (
                        SELECT SUM(pi.Quantity * p.BuyPrice)
                        FROM PurchaseItems pi
                        INNER JOIN Products p ON pi.ProductId = p.ProductId
                        WHERE pi.ModDate >= @FromDate 
                        AND pi.ModDate <= @ToDate 
                        AND pi.BranchId = @BranchId
                    ) 
                    + 
                    (
                        SELECT SUM(Amount)
                        FROM PaymentTransactions
                        WHERE InvoiceId IS NULL 
                        AND TransactionType = 'اضافة'
                        AND TransactionDate >= @FromDate 
                        AND TransactionDate <= @ToDate
                    ) AS TotalBuyCost,
                    SUM(pi.Quantity * p.SellPrice) AS TotalSellRevenue
                FROM 
                    PurchaseItems pi
                INNER JOIN 
                    Products p ON pi.ProductId = p.ProductId
                WHERE 
                    pi.ModDate >= @FromDate 
                    AND pi.ModDate <= @ToDate 
                    AND pi.BranchId = @BranchId
                GROUP BY pi.BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync<FinancialData?>(sql, new { FromDate = fromDate, ToDate = toDate, BranchId = branchId }).ConfigureAwait(false);
                return result;
            }
        }
    }
}
