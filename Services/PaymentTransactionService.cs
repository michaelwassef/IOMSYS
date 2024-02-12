using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.CodeAnalysis.Operations;

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

        public async Task<PaymentTransactionModel> GetPaymentTransactionByIdAsync(int transactionId)
        {
            var sql = @"SELECT * FROM PaymentTransactions WHERE TransactionId = @TransactionId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryFirstOrDefaultAsync<PaymentTransactionModel>(sql, new { TransactionId = transactionId }).ConfigureAwait(false);
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
            var sql = @"INSERT INTO PaymentTransactions (BranchId, PaymentMethodId, TransactionType, TransactionDate, Amount, Details, ModifiedDate, ModifiedUser) 
                        VALUES (@BranchId, @PaymentMethodId, @TransactionType, @TransactionDate, @Amount, @Details, @ModifiedDate, @ModifiedUser);
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
                            ModifiedDate = @ModifiedDate, ModifiedUser = @ModifiedUser 
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
