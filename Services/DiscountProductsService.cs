using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class DiscountProductsService : IDiscountProductsService
    {
        private readonly DapperContext _dapperContext;

        public DiscountProductsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<int> AddProductToDiscountAsync(DiscountProducts discountProducts)
        {
            var sql = @"INSERT INTO DiscountProducts (ProductId, DiscountId) VALUES (@ProductId, @DiscountId)";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { ProductId = discountProducts.ProductId, DiscountId = discountProducts.DiscountId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> RemoveProductFromDiscountAsync(DiscountProducts discountProducts)
        {
            var sql = @"DELETE FROM DiscountProducts WHERE ProductId = @ProductId AND DiscountId = @DiscountId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { ProductId = discountProducts.ProductId, DiscountId = discountProducts.DiscountId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
