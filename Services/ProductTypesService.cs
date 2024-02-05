using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class ProductTypesService : IProductTypesService
    {
        private readonly DapperContext _dapperContext;

        public ProductTypesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<ProductTypesModel>> GetAllProductTypesAsync()
        {
            var sql = @"SELECT * FROM ProductTypes";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductTypesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<ProductTypesModel?> SelectProductTypeByIdAsync(int productTypeId)
        {
            var sql = @"SELECT * FROM ProductTypes WHERE ProductTypeId = @ProductTypeId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<ProductTypesModel>(sql, new { ProductTypeId = productTypeId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertProductTypeAsync(ProductTypesModel productType)
        {
            var sql = @"INSERT INTO ProductTypes (ProductTypeName) VALUES (@ProductTypeName);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { productType.ProductTypeName }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateProductTypeAsync(ProductTypesModel productType)
        {
            var sql = @"UPDATE ProductTypes SET ProductTypeName = @ProductTypeName WHERE ProductTypeId = @ProductTypeId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { productType.ProductTypeName, productType.ProductTypeId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteProductTypeAsync(int productTypeId)
        {
            var sql = @"DELETE FROM ProductTypes WHERE ProductTypeId = @ProductTypeId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { ProductTypeId = productTypeId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
