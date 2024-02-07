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

        public async Task<bool> AuthenticateUserAsync1(AccessModel accessmodel)
        {
            var sql = @"SELECT COUNT(*) FROM Users WHERE UserName = @Username AND Password = @Password AND UserTypeId = @UserTypeId";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { UserName = accessmodel.UserName, Password = accessmodel.Password, UserTypeId = accessmodel.UserTypeId });
            return count > 0;
        }

        public async Task<AuthenticationResultModel> AuthenticateUserAsync(AccessModel accessmodel)
        {
            var sql = @"SELECT UserId FROM Users WHERE UserName = @Username AND Password = @Password AND UserTypeId = @UserTypeId";
            var userId = await _db.QuerySingleOrDefaultAsync<string>(sql, new { accessmodel.UserName, accessmodel.Password, accessmodel.UserTypeId });

            var isAuthenticated = !string.IsNullOrEmpty(userId);

            return new AuthenticationResultModel
            {
                IsAuthenticated = isAuthenticated,
                UserId = userId
            };
        }
    }
}
