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

        Task<decimal> CalculateAmountOwedByBranchAsync(int supplierId, int branchId);
        Task<IEnumerable<int>> GetNotFullyPaidInvoiceIdsAsync(int branchId);
        Task<PurchaseInvoicesModel> GetPurchaseInvoiceByIdAsync(int purchaseInvoiceId);
        Task<int> UpdatePurchaseInvoiceAsync(int purchaseInvoiceId, decimal paidUp, decimal remainder, bool isFullPaidUp);
        Task<decimal> ProcessInvoicesAndUpdateBalances(int fromBranchId, int toBranchId, decimal amountToSpend);

        Task<decimal> ProcessInvoicesAndUpdateBalancesBRANSHES(int fromBranchId, int toBranchId, decimal amountToSpend);

        Task<decimal> CalculateAmountOwedByBranchAsyncS(int CustomerId, int BranchId);
        Task<IEnumerable<int>> GetNotFullyPaidInvoiceIdsAsyncS(int branchId);
        Task<SalesInvoicesModel> GetSalesInvoiceByIdAsyncS(int SalesInvoiceId);
        Task<int> UpdateSalesInvoiceAsyncS(int SalesInvoiceId, decimal PaidUp, decimal Remainder, bool IsFullPaidUp);
        Task<decimal> ProcessInvoicesAndUpdateBalancesS(int fromBranchId, int toBranchId, decimal amountToSpend);
    }
}
