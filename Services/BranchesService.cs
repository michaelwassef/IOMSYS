using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class BranchesService : IBranchesService
    {
        private readonly DapperContext _dapperContext;

        public BranchesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<BranchesModel>> GetAllBranchesAsync()
        {
            var sql = @"SELECT B.*,U.UserName FROM Branches B
                        LEFT JOIN Users U ON B.BranchMangerId = U.UserId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<BranchesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<BranchesModel>> GetAllBranchesByUserAsync(int UserId)
        {
            var sql = @"SELECT B.BranchId,BI.BranchName FROM UserBranches B
                        INNER JOIN Branches BI ON BI.BranchId = B.BranchId
                        WHERE B.UserId = @UserId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<BranchesModel>(sql,new { UserId }).ConfigureAwait(false);
            }
        }

        public async Task<BranchesModel?> SelectBranchByIdAsync(int branchId)
        {
            var sql = @"SELECT B.*,U.UserName FROM Branches B 
                        LEFT JOIN Users U ON B.BranchMangerId = U.UserId
                        WHERE B.BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<BranchesModel>(sql, new { BranchId = branchId }).ConfigureAwait(false);
            }
        }

        public async Task<int?> SelectBranchIdByManagerIdAsync(int BranchMangerId)
        {
            var sql = @"SELECT B.BranchId 
                            FROM Branches B 
                            WHERE B.BranchMangerId = @BranchMangerId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<int?>(sql, new { BranchMangerId }).ConfigureAwait(false);
            }
        }

        public async Task<int?> SelectUserIdByBranchIdAsync(int BranchId)
        {
            var sql = @"SELECT B.BranchMangerId FROM Branches B 
                        WHERE B.BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<int?>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertBranchAsync(BranchesModel branch)
        {
            var sql = @"INSERT INTO Branches (BranchName, PhoneNumber, LandlinePhone, Address, BranchLogo, BranchMangerId) 
                        VALUES (@BranchName, @PhoneNumber, @LandlinePhone, @Address, @BranchLogo, @BranchMangerId);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { branch.BranchName,branch.PhoneNumber,branch.LandlinePhone,branch.Address,branch.BranchLogo,branch.BranchMangerId  }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateBranchAsync(BranchesModel branch)
        {
            var sql = @"UPDATE Branches SET BranchName = @BranchName, PhoneNumber = @PhoneNumber, LandlinePhone = @LandlinePhone,
                        Address = @Address, BranchLogo = @BranchLogo, BranchMangerId = @BranchMangerId WHERE BranchId = @BranchId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { branch.BranchName, branch.PhoneNumber, branch.LandlinePhone, branch.Address, branch.BranchLogo, branch.BranchMangerId, branch.BranchId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteBranchAsync(int branchId)
        {
            var sql = @"DELETE FROM Branches WHERE BranchId = @BranchId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { BranchId = branchId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
