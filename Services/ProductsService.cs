using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.CodeAnalysis;

namespace IOMSYS.Services
{
    public class ProductsService : IProductsService
    {
        private readonly DapperContext _dapperContext;

        public ProductsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<ProductsModel>> GetAllProductsAsync()
        {
            var sql = @"SELECT P.ProductId, P.ProductName, C.CategoryName, P.ProductPhoto, T.ProductTypeName, P.SellPrice, P.BuyPrice, P.MaxDiscount, P.DiscountsOn, P.Notes, P.CategoryId, P.ProductTypeId
                FROM Products P
                INNER JOIN Categories C ON C.CategoryId = P.CategoryId
                INNER JOIN ProductTypes T ON T.ProductTypeId = P.ProductTypeId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<ProductsModel?> SelectProductByIdAsync(int productId)
        {
            var sql = @"SELECT P.ProductId, P.ProductName, C.CategoryName, P.ProductPhoto, T.ProductTypeName, P.SellPrice, P.BuyPrice, P.MaxDiscount, P.DiscountsOn, P.Notes, P.CategoryId, P.ProductTypeId
                FROM Products P
                INNER JOIN Categories C ON C.CategoryId = P.CategoryId
                INNER JOIN ProductTypes T ON T.ProductTypeId = P.ProductTypeId
            WHERE ProductId = @ProductId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<ProductsModel>(sql, new { ProductId = productId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertProductAsync(ProductsModel product)
        {
            var sql = @"INSERT INTO Products (ProductId, ProductName, CategoryId, ProductPhoto, ProductTypeId, SellPrice, BuyPrice, MaxDiscount, DiscountsOn, Notes) 
                VALUES (@ProductId, @ProductName, @CategoryId, @ProductPhoto, @ProductTypeId, @SellPrice, @BuyPrice, @MaxDiscount, @DiscountsOn, @Notes);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    await db.ExecuteAsync(sql, new { product.ProductId, product.ProductName, product.CategoryId, product.ProductPhoto, product.ProductTypeId, product.SellPrice, product.BuyPrice, product.MaxDiscount, product.DiscountsOn, product.Notes }).ConfigureAwait(false);
                    return product.ProductId;
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateProductAsync(ProductsModel product)
        {
            var sql = @"UPDATE Products SET ProductName = @ProductName, CategoryId = @CategoryId, ProductPhoto = @ProductPhoto, ProductTypeId = @ProductTypeId, SellPrice = @SellPrice, BuyPrice = @BuyPrice, MaxDiscount = @MaxDiscount, DiscountsOn = @DiscountsOn, Notes = @Notes WHERE ProductId = @ProductId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { product.ProductName, product.CategoryId, product.ProductPhoto, product.ProductTypeId, product.SellPrice, product.BuyPrice, product.MaxDiscount, product.DiscountsOn, product.Notes, product.ProductId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteProductAsync(int productId)
        {
            var sql = @"DELETE FROM Products WHERE ProductId = @ProductId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { ProductId = productId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
        public async Task<int> UpdateProductBuyandSellPriceAsync(int ProductId,decimal BuyPrice,decimal SellPrice)
        {
            var sql = @"UPDATE Products SET BuyPrice = @BuyPrice,SellPrice=@SellPrice WHERE ProductId = @ProductId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { BuyPrice, SellPrice, ProductId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
