using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPaymentTransactionService
    {
        Task<IEnumerable<PaymentTransactionModel>> GetAllPaymentTransactionsAsync();
        Task<IEnumerable<PaymentTransactionModel>> LoadPaymentTransactionsByBranchAsync(int branchId);
        Task<PaymentTransactionModel> GetPaymentTransactionByIdAsync(int transactionId);
        Task<decimal> GetBranchAccountBalanceAsync(int branchId);
        Task<int> InsertPaymentTransactionAsync(PaymentTransactionModel transaction);
        Task<int> UpdatePaymentTransactionAsync(PaymentTransactionModel transaction);
        Task<int> DeletePaymentTransactionAsync(int transactionId);
    }
}
