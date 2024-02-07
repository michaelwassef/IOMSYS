using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IAccessService
    {
        Task<AuthenticationResultModel> AuthenticateUserAsync(AccessModel accessModel);
    }
}
