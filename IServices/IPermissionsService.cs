namespace IOMSYS.IServices
{
    public interface IPermissionsService
    {
        Task<bool> HasPermissionAsync(int userId, string controllerName, string actionName);
    }
}
