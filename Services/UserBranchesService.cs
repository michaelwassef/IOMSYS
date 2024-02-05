using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class UserBranchesService : IUserBranchesService
    {
        private readonly DapperContext _dapperContext;

        public UserBranchesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<int> AddUserToBranchAsync(UserBranchesModel userBranchesModel)
        {
            var sql = @"INSERT INTO UserBranches (UserId, BranchId) VALUES (@UserId, @BranchId)";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { UserId = userBranchesModel.UserId, BranchId = userBranchesModel.BranchId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> RemoveUserFromBranchAsync(UserBranchesModel userBranchesModel)
        {
            var sql = @"DELETE FROM UserBranches WHERE UserId = @UserId AND BranchId = @BranchId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { UserId = userBranchesModel.UserId, BranchId = userBranchesModel.BranchId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<IEnumerable<int>> GetBranchesForUserAsync(int userId)
        {
            var sql = @"SELECT BranchId FROM UserBranches WHERE UserId = @UserId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<int>(sql, new { UserId = userId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<int>> GetUsersForBranchAsync(int branchId)
        {
            var sql = @"SELECT UserId FROM UserBranches WHERE BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<int>(sql, new { BranchId = branchId }).ConfigureAwait(false);
            }
        }
    }
}
