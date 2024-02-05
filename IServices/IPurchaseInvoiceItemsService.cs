using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPurchaseInvoiceItemsService
    {
        Task<int> AddItemToPurchaseInvoiceAsync(PurchaseInvoiceItemsModel purchaseInvoiceItem);
        Task<int> RemoveItemFromPurchaseInvoiceAsync(PurchaseInvoiceItemsModel purchaseInvoiceItem);
    }
}
