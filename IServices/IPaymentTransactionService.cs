using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPaymentTransactionService
    {
        Task<IEnumerable<PaymentTransactionModel>> GetAllPaymentTransactionsAsync();
        Task<IEnumerable<PaymentTransactionModel>> LoadPaymentTransactionsByBranchAsync(int branchId);
        Task<IEnumerable<TransactionDetailModel>> LoadDetailsPaymentTransactionsByBranchAsync(int branchId, DateTime fromdate, DateTime todate);
        Task<PaymentTransactionModel> GetPaymentTransactionByIdAsync(int transactionId);
        Task<IEnumerable<PaymentTransactionModel>> GetPaymentTransactionsByInvoiceIdAsync(int invoiceId);
        Task<decimal> GetBranchAccountBalanceAsync(int branchId);
        Task<decimal> GetBranchAccountBalanceByPaymentAsync(int branchId, int paymentMethodId);
        Task<int> InsertPaymentTransactionAsync(PaymentTransactionModel transaction);
        Task<int> InsertPaymentTransactionAsync(TransactionDetailModel transaction);
        Task<int> UpdatePaymentTransactionAsync(PaymentTransactionModel transaction);
        Task<int> DeletePaymentTransactionAsync(int transactionId);
        Task<decimal> CalculateAmountOwedByBranchAsync(int supplierId, int branchId);
        Task<IEnumerable<int>> GetNotFullyPaidInvoiceIdsAsync(int branchId, int supplierId);
        Task<PurchaseInvoicesModel> GetPurchaseInvoiceByIdAsync(int purchaseInvoiceId);
        Task<int> UpdatePurchaseInvoiceAsync(int purchaseInvoiceId, decimal paidUp, decimal remainder, bool isFullPaidUp);
        Task<decimal> ProcessInvoicesAndUpdateBalances(int supplierId, int toBranchId, decimal amountToSpend);
        Task<decimal> ProcessInvoicesAndUpdateBalancesBRANSHES(int SupplierId, int toBranchId, decimal amountToSpend);
        Task<decimal> ProcessInvoicesAndUpdateBalancesS(int customerId, int toBranchId, decimal amountToSpend, int PaymentMethodId);
        Task<decimal> CalculateAmountOwedByBranchAsyncS(int customerId, int branchId);
        Task<IEnumerable<int>> GetNotFullyPaidInvoiceIdsAsyncS(int branchId, int customerId);
        Task<SalesInvoicesModel> GetSalesInvoiceByIdAsyncS(int salesInvoiceId);
        Task<int> UpdateSalesInvoiceAsyncS(int salesInvoiceId, decimal paidUp, decimal remainder, bool isFullPaidUp);
        Task RecordPaymentTransaction(PurchaseInvoicesModel model, int invoiceId);
        Task RecordPaymentTransaction(SalesInvoicesModel model, int invoiceId);
    }
}
