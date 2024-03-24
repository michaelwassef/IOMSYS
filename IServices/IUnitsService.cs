using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IUnitsService
    {
        Task<IEnumerable<UnitsModel>> GetAllUnitsAsync();
        Task<UnitsModel?> SelectUnitByIdAsync(int UnitId);
        Task<int> InsertUnitAsync(UnitsModel Unit);
        Task<int> UpdateUnitAsync(UnitsModel Unit);
        Task<int> DeleteUnitAsync(int UnitId);
    }
}
