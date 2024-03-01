using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPaymentTransactionService
    {
        Task<IEnumerable<PaymentTransactionModel>> GetAllPaymentTransactionsAsync();
        Task<IEnumerable<PaymentTransactionModel>> LoadPaymentTransactionsByBranchAsync(int branchId);
        Task<IEnumerable<TransactionDetailModel>> LoadDetailsPaymentTransactionsByBranchAsync(int branchId);
        Task<PaymentTransactionModel> GetPaymentTransactionByIdAsync(int transactionId);
        Task<PaymentTransactionModel> GetPaymentTransactionByInvoiceIdAsync(int InvoiceId);
        Task<decimal> GetBranchAccountBalanceAsync(int branchId);
        Task<decimal> GetBranchAccountBalanceByPaymentAsync(int BranchId, int PaymentMethodId);
        Task<int> InsertPaymentTransactionAsync(PaymentTransactionModel transaction);
        Task<int> UpdatePaymentTransactionAsync(PaymentTransactionModel transaction);
        Task<int> DeletePaymentTransactionAsync(int transactionId);
    }
}
