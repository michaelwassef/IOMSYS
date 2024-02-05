using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class UserTypesService : IUserTypesService
    {
        private readonly DapperContext _dapperContext;

        public UserTypesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<UserTypesModel>> GetAllUserTypesAsync()
        {
            var sql = @"SELECT * FROM UserTypes";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<UserTypesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<UserTypesModel?> SelectUserTypeByIdAsync(int userTypeId)
        {
            var sql = @"SELECT * FROM UserTypes WHERE UserTypeId = @UserTypeId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<UserTypesModel>(sql, new { UserTypeId = userTypeId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertUserTypeAsync(UserTypesModel userType)
        {
            var sql = @"INSERT INTO UserTypes (UserTypeName) VALUES (@UserTypeName);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { userType.UserTypeName }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateUserTypeAsync(UserTypesModel userType)
        {
            var sql = @"UPDATE UserTypes SET UserTypeName = @UserTypeName WHERE UserTypeId = @UserTypeId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { userType.UserTypeName, userType.UserTypeId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteUserTypeAsync(int userTypeId)
        {
            var sql = @"DELETE FROM UserTypes WHERE UserTypeId = @UserTypeId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { UserTypeId = userTypeId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
