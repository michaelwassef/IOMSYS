using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IBranchesService
    {
        Task<IEnumerable<BranchesModel>> GetAllBranchesAsync();
        Task<BranchesModel?> SelectBranchByIdAsync(int branchId);
        Task<IEnumerable<BranchesModel>> GetAllBranchesByUserAsync(int UserId);
        Task<int?> SelectBranchIdByManagerIdAsync(int BranchMangerId);
        Task<int> InsertBranchAsync(BranchesModel branch);
        Task<int> UpdateBranchAsync(BranchesModel branch);
        Task<int> DeleteBranchAsync(int branchId);
        Task<int?> SelectUserIdByBranchIdAsync(int BranchId);
    }
}
