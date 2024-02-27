using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ISalesItemsService
    {
        Task<IEnumerable<SalesItemsModel>> GetAllSalesItemsAsync();
        Task<IEnumerable<SalesItemsModel>> GetAllReturnSalesItemsAsync(int BranchId);
        Task<SalesItemsModel> GetSalesItemByIdAsync(int salesItemId);
        Task<IEnumerable<SalesItemsModel>> GetSaleItemsByInvoiceIdAsync(int InvoiceId);
        Task<int> InsertSalesItemAsync(SalesItemsModel salesItem);
        Task<int> UpdateSalesItemAsync(SalesItemsModel salesItem);
        Task<int> DeleteSalesItemAsync(int salesItemId);
        Task<int> ReturnSalesItemAsync(int salesItemId);
    }
}
