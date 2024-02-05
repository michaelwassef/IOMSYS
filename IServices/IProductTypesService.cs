using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IProductTypesService
    {
        Task<IEnumerable<ProductTypesModel>> GetAllProductTypesAsync();
        Task<ProductTypesModel?> SelectProductTypeByIdAsync(int productTypeId);
        Task<int> InsertProductTypeAsync(ProductTypesModel productType);
        Task<int> UpdateProductTypeAsync(ProductTypesModel productType);
        Task<int> DeleteProductTypeAsync(int productTypeId);
    }
}
