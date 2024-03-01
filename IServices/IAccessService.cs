using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IAccessService
    {
        Task<AuthenticationResultModel> AuthenticateUserAsync(AccessModel accessModel);
        Task<bool> CheckPassword(int UserId, string Password);
    }
}
