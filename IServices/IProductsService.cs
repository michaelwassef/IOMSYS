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
        Task<int> UpdateProductBuyandSellPriceAsync(int ProductId, decimal BuyPrice,decimal SellPrice);
        Task<IEnumerable<ProductsModel>> GetAllProductsInWarehouseAsync();
        Task<IEnumerable<ProductsModel>> GetAllProductsInWarehouseAsync(int BranchId);
        Task<IEnumerable<ProductsModel>> WarehouseMovementsAsync(int BranchId, DateTime fromdate, DateTime todate);
        Task<IEnumerable<ProductsModel>> GetMinQuantityProductsInWarehouseAsync(int BranchId);
        Task<IEnumerable<ProductsModel>> GetAvailableColorsForProduct(int productId);
        Task<IEnumerable<ProductsModel>> GetAvailableSizesForProduct(int productId);
        Task<decimal> GetAvailableQuantity(int productId, int colorId, int sizeId,int branchId);
        Task<string> GetWhereAvailableQuantityAsync(int productId, int colorId, int sizeId);
    }
}
