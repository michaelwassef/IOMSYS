using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class PurchaseInvoicesService : IPurchaseInvoicesService
    {
        private readonly DapperContext _dapperContext;

        public PurchaseInvoicesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<PurchaseInvoicesModel>> GetAllPurchaseInvoicesAsync()
        {
            var sql = @"
                SELECT pi.PurchaseInvoiceId, pi.TotalAmount, pi.PaidUp,pi.SupplierId,pi.BranchId,pi.PaymentMethodId, 
                pi.Remainder, s.SupplierName, b.BranchName, pm.PaymentMethodName, u.UserName, pi.PurchaseDate, pi.UserId, pi.PaidUpDate, pi.IsFullPaidUp, pi.Notes
                FROM PurchaseInvoices pi
                LEFT JOIN Suppliers s ON pi.SupplierId = s.SupplierId
                LEFT JOIN Branches b ON pi.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON pi.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON pi.UserId = u.UserId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PurchaseInvoicesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<PurchaseInvoicesModel>> GetAllPurchaseInvoicesByBranchAsync(int BranchId)
        {
            var sql = @"
                SELECT pi.PurchaseInvoiceId, pi.TotalAmount, pi.PaidUp, pi.SupplierId, pi.BranchId, pi.PaymentMethodId,
                pi.Remainder, s.SupplierName, b.BranchName, pm.PaymentMethodName, u.UserName, pi.PurchaseDate, pi.UserId, pi.PaidUpDate, pi.IsFullPaidUp, pi.Notes
                FROM PurchaseInvoices pi
                LEFT JOIN Suppliers s ON pi.SupplierId = s.SupplierId
                LEFT JOIN Branches b ON pi.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON pi.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON pi.UserId = u.UserId
                WHERE pi.BranchId = @BranchId ORDER BY pi.PurchaseDate DESC";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PurchaseInvoicesModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<PurchaseInvoicesModel>> GetNotPaidPurchaseInvoicesByBranchAsync(DateTime PaidUpDate, int BranchId)
        {
            var sql = @"
                    SELECT
                        PurchaseInvoiceId,
                        TotalAmount,
                        PaidUp,
                        Remainder,
                        SupplierId,
                        BranchId,
                        PaymentMethodId,
                        UserId,
                        PurchaseDate,
                        PaidUpDate,
                        IsFullPaidUp,
                        Notes
                    FROM
                        PurchaseInvoices
                    WHERE
                        PaidUpDate = @PaidUpDate
                        AND BranchId = @BranchId
                        AND TotalAmount > PaidUp ORDER BY PurchaseDate DESC;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PurchaseInvoicesModel>(sql, new { PaidUpDate, BranchId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<PurchaseInvoicesModel>> GetAllNotPaidPurchaseInvoicesByBranchAsync(int BranchId)
        {
            var sql = @"
                    SELECT
                        PurchaseInvoiceId,
                        TotalAmount,
                        PaidUp,
                        Remainder,
                        SupplierId,
                        BranchId,
                        PaymentMethodId,
                        UserId,
                        PurchaseDate,
                        PaidUpDate,
                        IsFullPaidUp,
                        Notes
                    FROM
                        PurchaseInvoices
                    WHERE
                        BranchId = @BranchId
                        AND TotalAmount > PaidUp ORDER BY PurchaseDate DESC;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PurchaseInvoicesModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }

        public async Task<PurchaseInvoicesModel> GetPurchaseInvoiceByIdAsync(int purchaseInvoiceId)
        {
            var sql = @"
                SELECT pi.PurchaseInvoiceId, pi.TotalAmount, pi.PaidUp,pi.SupplierId,pi.BranchId,pi.PaymentMethodId, pi.Remainder, s.SupplierName, 
                b.BranchName, pm.PaymentMethodName, u.UserName, pi.PurchaseDate, pi.UserId, pi.PaidUpDate, pi.IsFullPaidUp, pi.Notes
                FROM PurchaseInvoices pi
                LEFT JOIN Suppliers s ON pi.SupplierId = s.SupplierId
                LEFT JOIN Branches b ON pi.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON pi.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON pi.UserId = u.UserId
                WHERE pi.PurchaseInvoiceId = @PurchaseInvoiceId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<PurchaseInvoicesModel>(sql, new { PurchaseInvoiceId = purchaseInvoiceId }).ConfigureAwait(false);
            }
        }


        public async Task<int> InsertPurchaseInvoiceAsync(PurchaseInvoicesModel purchaseInvoice)
        {
            var sql = @"INSERT INTO PurchaseInvoices (TotalAmount, PaidUp, Remainder, SupplierId, BranchId, PaymentMethodId, UserId, PurchaseDate, PaidUpDate, IsFullPaidUp, Notes) 
                        VALUES (@TotalAmount, @PaidUp, @Remainder, @SupplierId, @BranchId, @PaymentMethodId, @UserId, @PurchaseDate, @PaidUpDate, @IsFullPaidUp, @Notes);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql, purchaseInvoice).ConfigureAwait(false);
            }
        }

        public async Task<int> UpdatePurchaseInvoiceAsync(PurchaseInvoicesModel purchaseInvoice)
        {
            var sql = @"UPDATE PurchaseInvoices SET TotalAmount = @TotalAmount, PaidUp = @PaidUp, Remainder = @Remainder, 
                        SupplierId = @SupplierId, BranchId = @BranchId, PaymentMethodId = @PaymentMethodId, UserId = @UserId, 
                        PurchaseDate = @PurchaseDate, PaidUpDate = @PaidUpDate, IsFullPaidUp = @IsFullPaidUp, Notes = @Notes     
                        WHERE PurchaseInvoiceId = @PurchaseInvoiceId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, purchaseInvoice).ConfigureAwait(false);
            }
        }

        public async Task<int> DeletePurchaseInvoiceAsync(int purchaseInvoiceId)
        {
            var sql = @"DELETE FROM PurchaseInvoices WHERE PurchaseInvoiceId = @PurchaseInvoiceId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { PurchaseInvoiceId = purchaseInvoiceId }).ConfigureAwait(false);
            }
        }
        public async Task<int> GetLastInvoiceIdAsync()
        {
            var sql = "SELECT TOP 1 PurchaseInvoiceId FROM PurchaseInvoices ORDER BY PurchaseInvoiceId DESC";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql).ConfigureAwait(false);
            }
        }

    }
}
