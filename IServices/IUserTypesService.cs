using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IUserTypesService
    {
        Task<IEnumerable<UserTypesModel>> GetAllUserTypesAsync();
        Task<UserTypesModel?> SelectUserTypeByIdAsync(int userTypeId);
        Task<int> InsertUserTypeAsync(UserTypesModel userType);
        Task<int> UpdateUserTypeAsync(UserTypesModel userType);
        Task<int> DeleteUserTypeAsync(int userTypeId);
    }
}
