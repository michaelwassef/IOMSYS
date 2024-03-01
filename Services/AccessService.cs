using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;
using System.Data;

namespace IOMSYS.Services
{
    public class AccessService : IAccessService
    {
        private readonly IDbConnection _db;

        public AccessService(DapperContext dapperContext)
        {
            _db = dapperContext.CreateConnection();
        }

        public async Task<AuthenticationResultModel> AuthenticateUserAsync(AccessModel accessmodel)
        {
            var sql = @"SELECT UserId, IsActive, UserTypeId FROM Users WHERE UserName = @Username AND Password = @Password";
            var user = await _db.QuerySingleOrDefaultAsync<(string UserId, bool IsActive, int UserTypeId)>(sql, new { accessmodel.UserName, accessmodel.Password });

            var isAuthenticated = !string.IsNullOrEmpty(user.UserId) && user.IsActive;

            return new AuthenticationResultModel
            {
                IsAuthenticated = isAuthenticated,
                UserId = user.UserId,
                IsActive = user.IsActive,
                UserTypeId = user.UserTypeId
            };
        }

        public async Task<bool> CheckPassword(int UserId, string Password)
        {
            var sql = @"SELECT COUNT(*) FROM Users WHERE UserId = @UserId AND Password = @Password";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { UserId, Password });
            return count > 0;
        }

    }
}
