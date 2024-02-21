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
                        PT.TransactionId,
                        CASE 
                            WHEN E.ExpensesId IS NOT NULL THEN 'مصروفات'
                            WHEN PI.PurchaseInvoiceId IS NOT NULL THEN 'فاتورة مشتريات'
                            WHEN SI.SalesInvoiceId IS NOT NULL THEN 'فاتورة مبيعات'
                            ELSE 'Unknown'
                        END AS InvoiceType,
                        COALESCE(E.ExpensesId, PI.PurchaseInvoiceId, SI.SalesInvoiceId) AS InvoiceId,
                        CASE 
                            WHEN E.ExpensesId IS NOT NULL THEN E.ExpensesName
                            WHEN PI.PurchaseInvoiceId IS NOT NULL THEN CONCAT('فاتورة مشتريات #', CAST(PI.PurchaseInvoiceId AS VARCHAR), ' - ', S.SupplierName)
                            WHEN SI.SalesInvoiceId IS NOT NULL THEN CONCAT('فاتورة مبيعات #', CAST(SI.SalesInvoiceId AS VARCHAR), ' - ', C.CustomerName)
                            ELSE 'Unknown Detail'
                        END AS InvoiceDetail,
                        COALESCE(E.PurchaseDate, PI.PurchaseDate, SI.SaleDate) AS InvoiceDate,
                        COALESCE(E.ExpensesAmount, PI.TotalAmount, SI.TotalAmount) AS Amount,
                        PT.BranchId,
                        PT.PaymentMethodId,
                        PT.TransactionType,
                        PT.TransactionDate,
                        PT.Details,
                        PT.ModifiedDate,
                        PT.ModifiedUser,
                        CASE 
                            WHEN E.ExpensesId IS NOT NULL THEN 'N/A'
                            WHEN PI.PurchaseInvoiceId IS NOT NULL THEN S.SupplierName
                            WHEN SI.SalesInvoiceId IS NOT NULL THEN C.CustomerName
                            ELSE 'Unknown Entity'
                        END AS EntityName
                    FROM 
                        PaymentTransactions PT
                    LEFT JOIN 
                        Expenses E ON PT.InvoiceId = E.ExpensesId
                    LEFT JOIN 
                        PurchaseInvoices PI ON PT.InvoiceId = PI.PurchaseInvoiceId
                    LEFT JOIN 
                        SalesInvoices SI ON PT.InvoiceId = SI.SalesInvoiceId
                    LEFT JOIN 
                        Suppliers S ON PI.SupplierId = S.SupplierId
                    LEFT JOIN 
                        Customers C ON SI.CustomerId = C.CustomerId
                    WHERE 
                        PT.BranchId = @BranchId;";
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
