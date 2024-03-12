using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class UsersService : IUsersService
    {
        private readonly DapperContext _dapperContext;

        public UsersService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<UsersModel>> GetAllUsersAsync()
        {
            var sql = @"SELECT U.UserId, U.UserName, U.PhoneNumber, U.Password, T.UserTypeName, U.UserTypeId, U.IsActive 
                        FROM Users U
                        INNER JOIN UserTypes T ON T.UserTypeId = U.UserTypeId
                        ORDER BY U.UserId
                        OFFSET 1 ROWS
                        FETCH NEXT 9999999 ROWS ONLY";

            //var sql = @"SELECT U.UserId, U.UserName, U.PhoneNumber, U.Password, T.UserTypeName ,U.UserTypeId, U.IsActive FROM Users U
            //            INNER JOIN UserTypes T ON T.UserTypeId = U.UserTypeId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<UsersModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<UsersModel?> SelectUserByIdAsync(int userId)
        {
            var sql = @"SELECT U.UserId, U.UserName, U.PhoneNumber, U.Password, T.UserTypeName, U.UserTypeId, U.IsActive FROM Users U
                        INNER JOIN UserTypes T ON T.UserTypeId = U.UserTypeId
                        WHERE UserId = @UserId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<UsersModel>(sql, new { UserId = userId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertUserAsync(UsersModel user)
        {
            var sql = @"INSERT INTO Users (UserName, PhoneNumber, Password, UserTypeId, IsActive) 
                        VALUES (@UserName, @PhoneNumber, @Password, @UserTypeId, @IsActive);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { user.UserName, user.PhoneNumber, user.Password, user.UserTypeId, user.IsActive }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateUserAsync(UsersModel user)
        {
            var sql = @"UPDATE Users SET UserName = @UserName, PhoneNumber = @PhoneNumber, Password = @Password, UserTypeId = @UserTypeId, IsActive = @IsActive WHERE UserId = @UserId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { user.UserName, user.PhoneNumber, user.Password, user.UserTypeId, user.UserId, user.IsActive }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteUserAsync(int userId)
        {
            var sql = @"DELETE FROM Users WHERE UserId = @UserId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { UserId = userId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<IEnumerable<PermissionModel>> GetAllPermissions()
        {
            var sql = @"SELECT * FROM Permissions";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PermissionModel>(sql).ConfigureAwait(false);
            }

        }

        public async Task<IEnumerable<int>> GetPermissionsForUserAsync(int userId)
        {
            var sql = @"SELECT PermissionId FROM UserPermissions WHERE UserId = @UserId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<int>(sql, new { UserId = userId }).ConfigureAwait(false);
            }
        }

        public async Task<bool> AddPermissionToUserAsync(UserPermissionModel newpermission)
        {
            var sql = @"INSERT INTO UserPermissions (UserId, PermissionId) VALUES (@UserId, @PermissionId)";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    var affectedRows = await db.ExecuteAsync(sql, new { newpermission.UserId, newpermission.PermissionId }).ConfigureAwait(false);
                    return affectedRows > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemovePermissionFromUserAsync(UserPermissionModel permission)
        {
            var sql = @"DELETE FROM UserPermissions WHERE UserId = @UserId AND PermissionId = @PermissionId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    var affectedRows = await db.ExecuteAsync(sql, new { permission.UserId, permission.PermissionId }).ConfigureAwait(false);
                    return affectedRows > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
