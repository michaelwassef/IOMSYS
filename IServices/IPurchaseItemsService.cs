using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPurchaseItemsService
    {
        Task<IEnumerable<PurchaseItemsModel>> GetAllPurchaseItemsAsync();
        Task<IEnumerable<PurchaseItemsModel>> GetAllFactoryItemsByBranchAsync(int branchId);
        Task<PurchaseItemsModel> GetPurchaseItemsByIDitemAsync(int id);
        Task<PurchaseItemsModel> GetPurchaseItemByIdAsync(int purchaseItemId);
        Task<PurchaseItemsModel> GetPurchaseItemWithoutInvoiceByIdAsync(int purchaseItemId);
        Task<IEnumerable<PurchaseItemsModel>> GetPurchaseItemsByInvoiceIdAsync(int InvoiceId);
        Task<int> InsertPurchaseItemAsync(PurchaseItemsModel purchaseItem);
        Task<int> UpdatePurchaseItemAsync(PurchaseItemsModel purchaseItem);
        Task<int> DeletePurchaseItemAsync(int purchaseItemId);
    }
}
