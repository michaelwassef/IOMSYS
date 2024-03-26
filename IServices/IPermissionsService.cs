namespace IOMSYS.IServices
{
    public interface IPermissionsService
    {
        Task<bool> HasPermissionAsync(int userId, string controllerName, string actionName);
        Task LogActionAsync(int userId, string action, string tableName, int recordId, string details, int branchId);
    }
}
