using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IUsersService
    {
        Task<IEnumerable<UsersModel>> GetAllUsersAsync(); 
        Task<UsersModel?> SelectUserByIdAsync(int userId);
        Task<int> InsertUserAsync(UsersModel user);
        Task<int> UpdateUserAsync(UsersModel user);
        Task<int> DeleteUserAsync(int userId);
        Task<IEnumerable<PermissionModel>> GetAllPermissions();
        Task<IEnumerable<int>> GetPermissionsForUserAsync(int userId);
        Task<bool> AddPermissionToUserAsync(UserPermissionModel newpermission);
        Task<bool> RemovePermissionFromUserAsync(UserPermissionModel permission);
    }
}
