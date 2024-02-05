using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IUserBranchesService
    {
        Task<int> AddUserToBranchAsync(UserBranchesModel userBranchesModel);
        Task<int> RemoveUserFromBranchAsync(UserBranchesModel userBranchesModel);
        Task<IEnumerable<int>> GetBranchesForUserAsync(int userId);
        Task<IEnumerable<int>> GetUsersForBranchAsync(int branchId);
    }
}
