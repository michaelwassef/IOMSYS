using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPurchaseInvoicesService
    {
        Task<IEnumerable<PurchaseInvoicesModel>> GetAllPurchaseInvoicesAsync();
        Task<PurchaseInvoicesModel> GetPurchaseInvoiceByIdAsync(int purchaseInvoiceId);
        Task<int> InsertPurchaseInvoiceAsync(PurchaseInvoicesModel purchaseInvoice);
        Task<int> UpdatePurchaseInvoiceAsync(PurchaseInvoicesModel purchaseInvoice);
        Task<int> DeletePurchaseInvoiceAsync(int purchaseInvoiceId);
    }
}
