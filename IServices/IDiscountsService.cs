using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IDiscountsService
    {
        Task<IEnumerable<DiscountsModel>> GetAllDiscountsAsync();
        Task<DiscountsModel?> SelectDiscountByIdAsync(int discountId);
        Task<int> InsertDiscountAsync(DiscountsModel discount);
        Task<int> UpdateDiscountAsync(DiscountsModel discount);
        Task<int> DeleteDiscountAsync(int discountId);
    }
}
