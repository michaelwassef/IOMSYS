using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class ExpensesService : IExpensesService
    {
        private readonly DapperContext _dapperContext;

        public ExpensesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<ExpenseModel>> GetAllExpensesAsync()
        {
            var sql = @"SELECT * FROM Expenses";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ExpenseModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<ExpenseModel>> GetAllExpensesByBranchAsync(int branchId)
        {
            var sql = @"SELECT * FROM Expenses Where BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ExpenseModel>(sql, new { BranchId = branchId }).ConfigureAwait(false);
            }
        }

        public async Task<ExpenseModel?> SelectExpenseByIdAsync(int expensesId)
        {
            var sql = @"SELECT * FROM Expenses WHERE ExpensesId = @ExpensesId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<ExpenseModel>(sql, new { ExpensesId = expensesId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertExpenseAsync(ExpenseModel expense)
        {
            var sql = @"INSERT INTO Expenses (ExpensesName, ExpensesAmount, Notes, PaymentMethodId, BranchId, UserId, PurchaseDate) 
                        VALUES (@ExpensesName, @ExpensesAmount, @Notes, @PaymentMethodId, @BranchId, @UserId, @PurchaseDate);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, expense).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateExpenseAsync(ExpenseModel expense)
        {
            var sql = @"UPDATE Expenses SET ExpensesName = @ExpensesName, ExpensesAmount = @ExpensesAmount, 
                        Notes = @Notes, PaymentMethodId = @PaymentMethodId, BranchId = @BranchId, 
                        UserId = @UserId, PurchaseDate = @PurchaseDate WHERE ExpensesId = @ExpensesId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, expense).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteExpenseAsync(int expensesId)
        {
            var sql = @"DELETE FROM Expenses WHERE ExpensesId = @ExpensesId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { ExpensesId = expensesId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
