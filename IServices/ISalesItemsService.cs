using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ISalesItemsService
    {
        Task<IEnumerable<SalesItemsModel>> GetAllSalesItemsAsync();
        Task<SalesItemsModel> GetSalesItemByIdAsync(int salesItemId);
        Task<int> InsertSalesItemAsync(SalesItemsModel salesItem);
        Task<int> UpdateSalesItemAsync(SalesItemsModel salesItem);
        Task<int> DeleteSalesItemAsync(int salesItemId);
    }
}
