using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ICategoriesService
    {
        Task<IEnumerable<CategoriesModel>> GetAllCategoriesAsync();
        Task<CategoriesModel?> SelectCategoryByIdAsync(int categoryId);
        Task<int> InsertCategoryAsync(CategoriesModel category);
        Task<int> UpdateCategoryAsync(CategoriesModel category);
        Task<int> DeleteCategoryAsync(int categoryId);
    }
}
