using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IDiscountProductsService
    {
        Task<int> AddProductToDiscountAsync(DiscountProducts discountProducts);
        Task<int> RemoveProductFromDiscountAsync(DiscountProducts discountProducts);
    }
}
