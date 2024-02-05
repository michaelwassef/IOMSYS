using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IColorsService
    {
        Task<IEnumerable<ColorsModel>> GetAllColorsAsync();
        Task<ColorsModel?> SelectColorByIdAsync(int colorId);
        Task<int> InsertColorAsync(ColorsModel color);
        Task<int> UpdateColorAsync(ColorsModel color);
        Task<int> DeleteColorAsync(int colorId);
    }
}
