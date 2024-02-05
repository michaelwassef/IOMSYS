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
            var sql = @"SELECT * FROM Branches";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<BranchesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<BranchesModel?> SelectBranchByIdAsync(int branchId)
        {
            var sql = @"SELECT * FROM Branches WHERE BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<BranchesModel>(sql, new { BranchId = branchId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertBranchAsync(BranchesModel branch)
        {
            var sql = @"INSERT INTO Branches (BranchName, PhoneNumber, LandlinePhone, Address) 
                        VALUES (@BranchName, @PhoneNumber, @LandlinePhone, @Address);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { branch.BranchName, branch.PhoneNumber, branch.LandlinePhone, branch.Address }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateBranchAsync(BranchesModel branch)
        {
            var sql = @"UPDATE Branches SET BranchName = @BranchName, PhoneNumber = @PhoneNumber, LandlinePhone = @LandlinePhone, Address = @Address WHERE BranchId = @BranchId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { branch.BranchName, branch.PhoneNumber, branch.LandlinePhone, branch.Address, branch.BranchId }).ConfigureAwait(false);
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
