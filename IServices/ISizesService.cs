using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ISizesService
    {
        Task<IEnumerable<SizesModel>> GetAllSizesAsync();
        Task<SizesModel?> SelectSizeByIdAsync(int sizeId);
        Task<int> InsertSizeAsync(SizesModel size);
        Task<int> UpdateSizeAsync(SizesModel size);
        Task<int> DeleteSizeAsync(int sizeId);
    }
}
