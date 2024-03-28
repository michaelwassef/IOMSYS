using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IExpensesService
    {
        Task<IEnumerable<ExpenseModel>> GetAllExpensesAsync();
        Task<ExpenseModel?> SelectExpenseByIdAsync(int expensesId);
        Task<IEnumerable<ExpenseModel>> GetAllExpensesByBranchAsync(int branchId);
        Task<IEnumerable<ExpenseModel>> GetAllExpensesByBranchAndDateAsync(int branchId, DateTime FromDate, DateTime ToDate);
        Task<int> InsertExpenseAsync(ExpenseModel expense);
        Task<int> UpdateExpenseAsync(ExpenseModel expense);
        Task<int> DeleteExpenseAsync(int expensesId);
    }
}
