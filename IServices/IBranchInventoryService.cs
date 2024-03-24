using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IBranchInventoryService
    {
        Task<IEnumerable<BranchInventoryModel>> GetAllBranchInventoriesAsync();
        Task<BranchInventoryModel?> GetInventoryByProductAndBranchAsync(int productId, int sizeId, int colorId, int branchId);
        Task<BranchInventoryModel?> GetInventoryByBranchAsync(int branchId);
        Task<int> UpdateInventoryQuantityAsync(BranchInventoryModel inventory);
        Task<int> AddOrUpdateInventoryAsync(BranchInventoryModel inventory);
        Task<int> AdjustInventoryQuantityAsync(int productId, int sizeId, int colorId, int branchId, decimal quantityAdjustment);
    }
}
