using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPurchaseInvoicesService
    {
        Task<IEnumerable<PurchaseInvoicesModel>> GetAllPurchaseInvoicesAsync();
        Task<PurchaseInvoicesModel> GetPurchaseInvoiceByIdAsync(int purchaseInvoiceId);
        Task<PurchaseInvoicesModel> GetPurchaseInvoiceBySalesIdAsync(int salesInvoiceId);
        Task<IEnumerable<PurchaseInvoicesModel>> GetAllPurchaseInvoicesByBranchAsync(int BranchId);
        Task<IEnumerable<PurchaseInvoicesModel>> GetAllPurchaseInvoicesByBranchAndDateAsync(int BranchId, DateTime FromDate, DateTime ToDate);
        Task<IEnumerable<PurchaseInvoicesModel>> GetNotPaidPurchaseInvoicesByBranchAsync(DateTime PaidUpDate, int BranchId);
        Task<IEnumerable<PurchaseInvoicesModel>> GetAllNotPaidPurchaseInvoicesByBranchAsync(int BranchId);
        Task<int> InsertPurchaseInvoiceAsync(PurchaseInvoicesModel purchaseInvoice);
        Task<int> UpdatePurchaseInvoiceAsync(PurchaseInvoicesModel purchaseInvoice);
        Task<int> UpdatePurchaseInvoiceAsync2(PurchaseInvoicesModel purchaseInvoice);
        Task<int> DeletePurchaseInvoiceAsync(int purchaseInvoiceId);
        Task<int> GetLastInvoiceIdAsync();
    }
}
