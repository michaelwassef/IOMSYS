using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;

namespace IOMSYS.Services
{
    public class PermissionsService : IPermissionsService
    {
        private readonly DapperContext _dapperContext;
        public PermissionsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<bool> HasPermissionAsync(int userId, string controllerName, string actionName)
        {
            var sql = @"
            SELECT COUNT(1)
            FROM UserPermissions UP
            INNER JOIN Permissions P ON UP.PermissionId = P.PermissionId
            WHERE UP.UserId = @UserId
            AND P.ControllerName = @ControllerName
            AND P.ActionName = @ActionName";

            using (var db = _dapperContext.CreateConnection())
            {
                var count = await db.ExecuteScalarAsync<int>(sql, new { UserId = userId, ControllerName = controllerName, ActionName = actionName });
                return count > 0;
            }
        }
    }
}
