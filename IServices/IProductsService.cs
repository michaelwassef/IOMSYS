using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IProductsService
    {
        Task<IEnumerable<ProductsModel>> GetAllProductsAsync();
        Task<ProductsModel?> SelectProductByIdAsync(int productId);
        Task<int> InsertProductAsync(ProductsModel product);
        Task<int> UpdateProductAsync(ProductsModel product);
        Task<int> DeleteProductAsync(int productId);
    }
}
