using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ISalesInvoiceItemsService
    {
        Task<int> SelectInvoiceConnectToItemAsync(int SalesItemId);
        Task<int> AddSalesItemToInvoiceAsync(SalesInvoiceItemsModel salesInvoiceItem);
        Task<int> RemoveSalesItemFromInvoiceAsync(SalesInvoiceItemsModel salesInvoiceItem);
    }
}
