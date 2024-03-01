using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly DapperContext _dapperContext;

        public PaymentTransactionService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<PaymentTransactionModel>> GetAllPaymentTransactionsAsync()
        {
            var sql = @"SELECT * FROM PaymentTransactions";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PaymentTransactionModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<PaymentTransactionModel>> LoadPaymentTransactionsByBranchAsync(int branchId)
        {
            var sql = @"SELECT * FROM PaymentTransactions WHERE BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PaymentTransactionModel>(sql, new { BranchId = branchId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<TransactionDetailModel>> LoadDetailsPaymentTransactionsByBranchAsync(int branchId)
        {
            var sql = @"SELECT 
                P.TransactionId,
                P.BranchId,
                P.PaymentMethodId,
                P.TransactionType,
                P.TransactionDate as InvoiceDate,
                P.Amount,
                P.Details,
                P.ModifiedDate,
                P.ModifiedUser,
                P.InvoiceId,
                b.BranchName,
                u.UserName,
                pm.PaymentMethodName,
                CASE 
                    WHEN P.TransactionType = 'اضافة' AND EXISTS (SELECT 1 FROM SalesInvoices WHERE SalesInvoiceId = P.InvoiceId) THEN 'فاتورة مبيعات'
                    WHEN EXISTS (SELECT 1 FROM PurchaseInvoices WHERE PurchaseInvoiceId = P.InvoiceId) THEN 'فاتورة مشتريات'
                    WHEN P.TransactionType = 'خصم' AND EXISTS (SELECT 1 FROM SalesInvoices WHERE SalesInvoiceId = P.InvoiceId) THEN 'مرتجع فاتورة مبيعات'
                    WHEN EXISTS (SELECT 1 FROM Expenses WHERE ExpensesId = P.InvoiceId) THEN 'مصروفات'
                    ELSE P.TransactionType 
                END AS InvoiceType,
               CASE 
                    WHEN EXISTS (SELECT 1 FROM SalesInvoices WHERE SalesInvoiceId = P.InvoiceId) THEN c.CustomerName
                    WHEN EXISTS (SELECT 1 FROM PurchaseInvoices WHERE PurchaseInvoiceId = P.InvoiceId) THEN s.SupplierName
                    ELSE NULL 
                END AS EntityName
            FROM 
                IOMSYS.dbo.PaymentTransactions P
            INNER JOIN 
                Branches b ON P.BranchId = b.BranchId
            INNER JOIN 
                Users u ON P.ModifiedUser = u.UserId
            INNER JOIN 
                PaymentMethods pm ON P.PaymentMethodId = pm.PaymentMethodId
            LEFT JOIN 
                SalesInvoices si ON si.SalesInvoiceId = P.InvoiceId
            LEFT JOIN 
                PurchaseInvoices pi ON pi.PurchaseInvoiceId = P.InvoiceId
            LEFT JOIN 
                Customers c ON c.CustomerId = si.CustomerId
            LEFT JOIN 
                Suppliers s ON s.SupplierId = pi.SupplierId
            WHERE
                P.BranchId = @BranchId;";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<TransactionDetailModel>(sql, new { BranchId = branchId }).ConfigureAwait(false);
            }
        }

        public async Task<PaymentTransactionModel> GetPaymentTransactionByIdAsync(int transactionId)
        {
            var sql = @"SELECT * FROM PaymentTransactions WHERE TransactionId = @TransactionId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryFirstOrDefaultAsync<PaymentTransactionModel>(sql, new { TransactionId = transactionId }).ConfigureAwait(false);
            }
        }

        public async Task<PaymentTransactionModel> GetPaymentTransactionByInvoiceIdAsync(int InvoiceId)
        {
            var sql = @"SELECT * FROM PaymentTransactions WHERE InvoiceId = @InvoiceId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryFirstOrDefaultAsync<PaymentTransactionModel>(sql, new { InvoiceId }).ConfigureAwait(false);
            }
        }

        public async Task<decimal> GetBranchAccountBalanceAsync(int branchId)
        {
            var sql = @"
                SELECT COALESCE(SUM(CASE WHEN TransactionType = 'اضافة' THEN Amount ELSE -Amount END), 0) AS Balance
                FROM PaymentTransactions 
                WHERE BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var balance = await db.QueryFirstOrDefaultAsync<decimal?>(sql, new { BranchId = branchId }).ConfigureAwait(false);
                if (balance.HasValue)
                {
                    return balance.Value;
                }
                else
                {
                    throw new Exception("Balance is null for the given branch.");
                }
            }
        }

        public async Task<decimal> GetBranchAccountBalanceByPaymentAsync(int BranchId, int PaymentMethodId)
        {
            var sql = @"
                SELECT COALESCE(SUM(CASE WHEN TransactionType = 'اضافة' THEN Amount ELSE -Amount END), 0) AS Balance
                FROM PaymentTransactions 
                WHERE BranchId = @BranchId AND PaymentMethodId = @PaymentMethodId";

            using (var db = _dapperContext.CreateConnection())
            {
                var balance = await db.QueryFirstOrDefaultAsync<decimal?>(sql, new { BranchId , PaymentMethodId }).ConfigureAwait(false);
                if (balance.HasValue)
                {
                    return balance.Value;
                }
                else
                {
                    throw new Exception("Balance is null for the given branch.");
                }
            }
        }

        public async Task<int> InsertPaymentTransactionAsync(PaymentTransactionModel transaction)
        {
            var sql = @"INSERT INTO PaymentTransactions (BranchId, PaymentMethodId, TransactionType, TransactionDate, Amount, Details, ModifiedDate, ModifiedUser, InvoiceId) 
                        VALUES (@BranchId, @PaymentMethodId, @TransactionType, @TransactionDate, @Amount, @Details, @ModifiedDate, @ModifiedUser, @InvoiceId);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, transaction).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdatePaymentTransactionAsync(PaymentTransactionModel transaction)
        {
            var sql = @"UPDATE PaymentTransactions 
                        SET BranchId = @BranchId, PaymentMethodId = @PaymentMethodId, TransactionType = @TransactionType, 
                            TransactionDate = @TransactionDate, Amount = @Amount, Details = @Details, 
                            ModifiedDate = @ModifiedDate, ModifiedUser = @ModifiedUser , InvoiceId = @InvoiceId
                        WHERE TransactionId = @TransactionId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, transaction).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeletePaymentTransactionAsync(int transactionId)
        {
            var sql = @"DELETE FROM PaymentTransactions WHERE TransactionId = @TransactionId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { TransactionId = transactionId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
