using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IAccessService
    {
        Task<bool> AuthenticateUserAsync(AccessModel accessmodel);
    }
}
