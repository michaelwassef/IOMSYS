using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IBranchesService
    {
        Task<IEnumerable<BranchesModel>> GetAllBranchesAsync();
        Task<BranchesModel?> SelectBranchByIdAsync(int branchId);
        Task<int> InsertBranchAsync(BranchesModel branch);
        Task<int> UpdateBranchAsync(BranchesModel branch);
        Task<int> DeleteBranchAsync(int branchId);
    }
}
